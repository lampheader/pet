#undef DEBUG
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.IO;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Net.Http.Json;
using System.Linq.Expressions;
using static System.Net.WebRequestMethods;
using System.Diagnostics.Metrics;
using System.Reflection;
using System.Text;
using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;

using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Collections.Generic;
using hh_db.DB_Models;
using System.Data;
using Microsoft.VisualBasic;
using Microsoft.Data.SqlClient;

const string http_resource = "https://krasnoyarsk.hh.ru/";

const string http_resource_search = "https://krasnoyarsk.hh.ru/search/vacancy?L_save_area=true&text=&excluded_text=&professional_role=156&professional_role=160&professional_role=10&professional_role=12&professional_role=150&professional_role=25&professional_role=165&professional_role=34&professional_role=36&professional_role=73&professional_role=155&professional_role=96&professional_role=164&professional_role=104&professional_role=157&professional_role=107&professional_role=112&professional_role=113&professional_role=148&professional_role=114&professional_role=116&professional_role=121&professional_role=124&professional_role=125&professional_role=126&industry=7&area=54&salary=&currency_code=RUR&experience=doesNotMatter&order_by=relevance&search_period=0&items_on_page=100&hhtmFrom=vacancy_search_filter";

using HttpClient client = new();

List<string>? list_vacancies = new();
List<string>? list_pages = new();
Statistics stat = new();
stat.startTime = DateTime.UtcNow;

HttpResponseMessage response = new();
string responseData = string.Empty;

try
{
    response = await client.GetAsync(http_resource_search);
    response.EnsureSuccessStatusCode();
    var html_text = await response.Content.ReadAsStringAsync();
    responseData = System.Web.HttpUtility.HtmlDecode(html_text);
}
catch (HttpRequestException e)
{
    Console.WriteLine($"Ошибка при запросе: {e.Message}");
}

Console.WriteLine($"Сбор вакансий...");

var pages = pages_regex().Matches(responseData);

foreach (Match match in pages)
{
    string buf = pages_link_regex().Match(match.Value).Value;
    buf = buf[7..^1];
    list_pages.Add(http_resource + buf);
}

if (list_pages.Count == 0) list_pages.Add(http_resource_search);

foreach (var pg in list_pages)
{
    try
    {
        response = await client.GetAsync(pg);
        response.EnsureSuccessStatusCode();
        var html_text = await response.Content.ReadAsStringAsync();
        responseData = System.Web.HttpUtility.HtmlDecode(html_text);
    }
    catch (HttpRequestException e)
    {
        Console.WriteLine($"Ошибка при запросе: {e.Message}");
    }

    string text_vacancies = vacancies_regex().Match(responseData).Value;
    MatchCollection vacancies = vacancies_id_regex().Matches(text_vacancies);

    foreach (Match match in vacancies)
        list_vacancies.Add(match.Value);
}

stat.recordCount = list_vacancies.Count;
Console.WriteLine($"Кол-во вакансий: {list_vacancies.Count}.");
Console.WriteLine($"Сбор информации и занесение в базу данных...");

#if DEBUG
using (StreamWriter sw = new("vacation.txt"))
#endif

foreach (var vac in list_vacancies)
{

    try
    {
        response = await client.GetAsync("https://krasnoyarsk.hh.ru/vacancy/" + vac);
        response.EnsureSuccessStatusCode();
        var html_text = await response.Content.ReadAsStringAsync();
        responseData = System.Web.HttpUtility.HtmlDecode(html_text);
    }
    catch (HttpRequestException e)
    {
        Console.WriteLine($"Ошибка при запросе: {e.Message}");
    }

    Match content_match = vacancy_content_regex().Match(responseData);
    if (!content_match.Success)
    {
        stat.recordNoAdded++;
    }

    var content = content_match.Value;
    content = content.Replace("<script type=\"application/ld+json\">", string.Empty)
                     .Replace("</script>", string.Empty);

    StringBuilder content_br = new(content);

    MatchCollection err_str = delete_fsil_symbol_regex().Matches(content);
    foreach (Match err in err_str)
    {
        int counter = 0, last_idx = 0;
        for (int idx = err.Index; idx < err.Index + err.Length; idx++)
        {
            if (content_br[idx] == '"')
            {
                counter++;
                if (counter > 3)
                {
                    content_br[idx] = '¦';
                    last_idx = idx;
                }
            }
            if (content_br[idx] == '\\') content_br[idx] = '¦';
        }

        content_br[last_idx] = '"';
    }

    content = content_br.ToString();
    content = content.Replace("¦", string.Empty);

    Json_Serializer.JSON_Models.Vacancy? vacancy = new();

    try
    {
        vacancy = JsonSerializer.Deserialize<Json_Serializer.JSON_Models.Vacancy>(content);
    }
    catch (JsonException e)
    {
        Console.WriteLine($"Ошибка при десериализации: {e.Message}\nvac_id={vac}");
    }

    var exp_match = Regex.Match(responseData, @"Требуемый опыт: .+?\.", RegexOptions.Singleline);
    var exp = exp_match.Value;

    exp = (exp_match.Success) ? exp[16..^1] : null;

    var desc = Regex.Matches(vacancy.description, @">[^<>]+?<", RegexOptions.Singleline);//-------
    vacancy.description = string.Empty;


    foreach (Match match in desc)
    {
        string buf = match.Value;
        buf = buf[1..^1];
        if (!Regex.IsMatch(buf, @"^\s+$"))
            vacancy.description += buf + "\n";
    }

    var address = Regex.Match(responseData, @"<span data-qa=""vacancy-view-raw-address"">.*?<\/span>", RegexOptions.Singleline).Value;
    address = address.Replace("<span data-qa=\"vacancy-view-raw-address\">", string.Empty)
                     .Replace("</span>", string.Empty)
                     .Replace("<!-- -->", string.Empty)
                     .Replace(vacancy.jobLocation.address.addressLocality + ", ", string.Empty);

    vacancy.jobLocation.address.streetAddress = address;

    var skills = Regex.Matches(responseData, @"<li data-qa=""skills-element"">.*?</li>", RegexOptions.Singleline);

    List<string?>? list_skills = new();
    foreach (Match match in skills)
    {
        string buf = Regex.Match(match.Value, @">[^<>]+<").Value;
        buf = buf[1..^1];
        list_skills.Add(buf);
    }

    using (TestVacanciesDbContext db = new())
    {
        try
        {
            var record_exists = db.Identifiers.Where(i => i.Value.ToString() == vac).ToList();
            if (record_exists.Count != 0)
            {
                var ex_record = db.Vacancies.Where(v => v.Id.ToString() == vac).ToList().First();

                ex_record.DateStatus.LastUpdate = DateTime.UtcNow;
                ex_record.DateStatus.ValIdThrough = vacancy.validThrough;
                db.SaveChanges();
                stat.recordUpdated++;
                continue;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n{vac}\n{ex.Message}");
            throw;
        }
        using (var transaction = db.Database.BeginTransaction())
        {
            try
            {
                var t_address = new Address()
                {
                    Type = vacancy.jobLocation.address.type,
                    AddressLocality = vacancy.jobLocation.address.addressLocality,
                    AddressRegion = vacancy.jobLocation.address.addressRegion,
                    AddressCountry = vacancy.jobLocation.address.addressCountry,
                    StreetAddress = vacancy.jobLocation.address.streetAddress
                };
                db.Addresses.Add(t_address);

                var t_applocreq = new ApplicantsLocationRequirement()
                {
                    Type = vacancy.applicantLocationRequirements.type,
                    Name = vacancy.applicantLocationRequirements.name
                };
                db.ApplicantsLocationRequirements.Add(t_applocreq);

                Value? t_val = null;
                if (vacancy.baseSalary is not null)
                {
                    t_val = new Value()
                    {
                        Type = vacancy.baseSalary.value.type,
                        UnitText = vacancy.baseSalary.value.unitText,
                        MinValue = vacancy.baseSalary.value.minValue,
                        MaxValue = vacancy.baseSalary.value.maxValue
                    };
                    db.Values.Add(t_val);
                }

                var t_hirorg = new HiringOrganization()
                {
                    Type = vacancy.hiringOrganization.type,
                    Name = vacancy.hiringOrganization.name
                };
                db.HiringOrganizations.Add(t_hirorg);

                var t_ident = new Identifier()
                {
                    Value = vacancy.identifier.value,
                    Type = vacancy.identifier.type,
                    Name = vacancy.identifier.name
                };
                db.Identifiers.Add(t_ident);

                db.SaveChanges();

                BaseSalary? t_basesal = null;
                if (vacancy.baseSalary is not null)
                {
                    t_basesal = new BaseSalary()
                    {
                        Type = vacancy.baseSalary.type,
                        Currency = vacancy.baseSalary.currency,
                        ValueId = t_val.Id
                    };

                    db.BaseSalarys.Add(t_basesal);
                };

                var t_jobloc = new JobLocation()
                {
                    Type = vacancy.hiringOrganization.type,
                    AddressId = t_address.Id
                };
                db.JobLocations.Add(t_jobloc);

                var t_datest = new DateStatus()
                {
                    DatePosted = vacancy.datePosted,
                    ValIdThrough = vacancy.validThrough,
                    LastUpdate = DateTime.UtcNow
                };
                db.DateStatuses.Add(t_datest);

                db.SaveChanges();

                list_skills = list_skills.Distinct(StringComparer.OrdinalIgnoreCase).ToList();

                var lower_list_skills = list_skills.Select(skill => skill.ToLower()).ToList();

                var skillsToAdd = list_skills.Select(name => new Skill { Name = name }).ToList();

                var existingNames = db.Skills
                                      .Where(s => lower_list_skills.Contains(s.Name.ToLower()))
                                      .Select(s => s.Name.ToLower())
                                      .ToHashSet();

                var uniqueSkills = skillsToAdd
                    .Where(skill => !existingNames.Contains(skill.Name.ToLower()))
                    .ToList();

                if (uniqueSkills.Count > 0)
                {
                    db.Skills.AddRange(uniqueSkills);
                    db.SaveChanges();
                }

                var t_vac = new Vacancy()
                {
                    Id = t_ident.Value,
                    Description = vacancy.description,
                    Title = vacancy.title,
                    DateStatusId = t_datest.Id,
                    Experience = exp,
                    HiringOrganizationId = t_hirorg.Id,
                    JobLocationId = t_jobloc.Id,
                    ApplicantLocationRequirementsId = t_applocreq.Id,
                    EmploymentType = vacancy.employmentType,
                    BaseSalaryId = t_basesal?.Id,
                    Skills = uniqueSkills
                };
                db.Vacancies.Add(t_vac);

                db.SaveChanges();

                transaction.Commit();
                stat.recordAdded++;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n{vac}\n{ex.Message}");
                transaction.Rollback();
                throw;
            }
        };
    }


#if DEBUG
        sw.WriteLine("---");
        sw.WriteLine($"Job ID: {vac}");
        sw.WriteLine($"Job exp: {exp}");
        sw.WriteLine($"Job Title: {vacancy.title}");
        sw.WriteLine($"Company: {vacancy.hiringOrganization.name}");
        sw.WriteLine($"Location: {vacancy.jobLocation.address.addressLocality} {vacancy.jobLocation.address.streetAddress}");
        sw.WriteLine($"Salary: {vacancy?.baseSalary?.value?.minValue}-{vacancy?.baseSalary?.value?.maxValue} {vacancy?.baseSalary?.currency}");
        sw.Write("Skills: ");
        foreach (var vac_id in list_skills)
            sw.Write("\"" + vac_id + "\", ");

        sw.WriteLine("\n");
        sw.WriteLine($"Job Description: \n{vacancy?.description}");
        sw.WriteLine("---");
#endif
}

using (TestVacanciesDbContext db = new())
    try
    {
        db.Database.ExecuteSqlRaw("UpdateStatuses @LastUpdateTime", new SqlParameter()
        {
            ParameterName = "@LastUpdateTime",
            SqlDbType = System.Data.SqlDbType.DateTime,
            Value = stat.startTime
        });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"{ex.Message}");
        throw;
    }


Console.WriteLine("\nСбор информации закончен.");
stat.recordNoUpdated = stat.recordCount - (stat.recordAdded+stat.recordUpdated+stat.recordNoAdded);
stat.endTime = DateTime.UtcNow;
stat.timePassed = stat.endTime - stat.startTime;
Console.WriteLine($"\nСтатистика:\nОбщее кол-во вакансий:{stat.recordCount}.\nКол-во добавленных вакансий:{stat.recordAdded}.");
Console.WriteLine($"Кол-во обновлённых вакансий:{stat.recordUpdated}.\nКол-во не добавленных вакансий:{stat.recordNoAdded}.");
Console.WriteLine($"Кол-во не обновлённых вакансий:{stat.recordNoUpdated}.\nВремени прошло:{stat.timePassed}.");



#if DEBUG
Console.WriteLine("Открытие файла...");
System.Diagnostics.Process process = new();
System.Diagnostics.ProcessStartInfo startInfo = new()
{
    WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal,
    FileName = "notepad.exe",
    Arguments = "\"C:\\Users\\lamp_head\\source\\repos\\hh_db\\bin\\Debug\\net8.0\\vacation.txt\""
};
process.StartInfo = startInfo;
process.Start();
Console.WriteLine("Файл открыт.");
#endif

partial class Program
{
    [GeneratedRegex(@"<a data-qa=""pager-page"".+?</a>", RegexOptions.Singleline)]
    private static partial Regex pages_regex();

    [GeneratedRegex(@"href="".+""")]
    private static partial Regex pages_link_regex();

    [GeneratedRegex(@"""userLabelsForVacancies"":\{(""\d+"":\[\],?)*\}", RegexOptions.Singleline)]
    private static partial Regex vacancies_regex();

    [GeneratedRegex(@"\d+", RegexOptions.Singleline)]
    private static partial Regex vacancies_id_regex();

    [GeneratedRegex(@"<script type=""application\/ld\+json"">.*?<\/script>", RegexOptions.Singleline)]
    private static partial Regex vacancy_content_regex();

    [GeneratedRegex(@""".+"": ""[^""\\]*[""\\].*"",?")]
    private static partial Regex delete_fsil_symbol_regex();
}

struct Statistics
{
    internal int recordNoUpdated;
    internal int recordUpdated;
    internal int recordAdded;
    internal int recordNoAdded;
    internal int recordCount;
    internal DateTime startTime;
    internal DateTime endTime;
    internal TimeSpan timePassed;
};
