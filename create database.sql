USE master;
GO
IF DB_ID (N'test_vacancies_db') IS NOT NULL
begin
ALTER DATABASE test_vacancies_db SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
DROP DATABASE test_vacancies_db ;
end
go
create database test_vacancies_db;
go
use test_vacancies_db;
go

begin tran create_db;

create table Addresses
(
	Id integer not null IDENTITY,
    type nvarchar(50),
	addressLocality nvarchar(50),
	addressRegion nvarchar(50),
	addressCountry nvarchar(50),
	streetAddress nvarchar(80),
	constraint addr_p_key primary key (Id)
);

create table ApplicantsLocationRequirements
(
	Id integer not null IDENTITY,
    type nvarchar(50),
	name nvarchar(60),
	constraint appLocReq_p_key primary key (Id)
);

create table Value
(
	Id integer not null IDENTITY,
    type nvarchar(50),
	unitText nvarchar(50),
	minValue integer null default null,
	maxValue integer null default null,
	constraint Value_p_key primary key (Id),
	constraint minVal_is_pos check(maxValue>=0),
	constraint maxVal_is_pos check(maxValue>=0),
	constraint minmaxVal_ch check(maxValue>=minValue),
);

create table BaseSalarys
(
	Id integer not null IDENTITY,
    type nvarchar(50),
	currency nvarchar(50),
	valueId int not null,
	constraint BaseSal_p_key primary key (Id),
	constraint BaseSal_value_f_key foreign key (valueId) references Value (Id) on delete cascade
);

create table HiringOrganizations
(
	Id integer not null IDENTITY,
    type nvarchar(50),
	name nvarchar(300),
	constraint HirOrg_p_key primary key (Id)
);

create table Identifiers
(
	value integer not null,
    type nvarchar(50),
	name nvarchar(300),
	constraint Id_p_key primary key (value)
);

create table JobLocations
(
	Id integer not null IDENTITY,
    type nvarchar(50),
	addressId integer null default null,
	constraint JobLocId_p_key primary key (Id),
	constraint addId_JobLoc_f_key foreign key (addressId) references Addresses (Id) on delete cascade
);

create table Skills
(
	Id integer not null IDENTITY,
    name nvarchar(100),
	constraint Skills_p_key primary key (Id),
	constraint skillName_uniq unique (name)
);

create table DateStatuses
(
	Id int not null IDENTITY,
	datePosted DateTime,
    valIdThrough DateTime,
	lastUpdate DateTime,
	status nvarchar(20) default 'null',
	constraint DateStatus_p_key primary key (Id),
	constraint date_ch check(datePosted<=valIdThrough)
);

create table Vacancies
(
	Id int not null,
	description nvarchar(max),
    title nvarchar(300),
	experience nvarchar(20),
    HiringOrganizationId int,
	DateStatusId int,
    JobLocationId int,
    ApplicantLocationRequirementsId int,
    employmentType nvarchar(50),
    BaseSalaryId int null default null,
	constraint Vacancies_p_key primary key (Id),
	constraint DateStId_f_key foreign key (DateStatusId) references DateStatuses (Id) on delete cascade,
	constraint Id_f_key foreign key (Id) references Identifiers (value) on delete cascade,
	constraint HirOrgId_f_key foreign key (HiringOrganizationId) references hiringOrganizations (Id) on delete cascade,
	constraint AppLocReq_Id_f_key foreign key (ApplicantLocationRequirementsId) references applicantsLocationRequirements (Id) on delete cascade,
	constraint BasSalId_val_f_key foreign key (BaseSalaryId) references baseSalarys (Id) on delete cascade,
	constraint JobLocId_f_key foreign key (JobLocationId) references JobLocations (Id) on delete cascade,
);

create table VacancySkills
(
	VacancyId int not null,
	SkillId int not null,
	constraint VacancySkills_p_key primary key (VacancyId,SkillId),
	constraint Vac_f_key foreign key (VacancyId) references Vacancies (Id) on delete NO ACTION,
	constraint Skill_f_key foreign key (SkillId) references Skills (Id) on delete NO ACTION
);


commit tran create_db;

use test_vacancies_db;

go
CREATE PROCEDURE UpdateStatuses
    @LastUpdateTime DATETIME
AS
BEGIN
    UPDATE DateStatuses
    SET status = CASE
        WHEN lastUpdate < @LastUpdateTime THEN 'Deleted'
        WHEN lastUpdate >= @LastUpdateTime AND lastUpdate <= valIdThrough THEN 'Active'
        WHEN lastUpdate > valIdThrough THEN 'Deleted?'
        ELSE 'error?'
    END
END;

--exec UpdateStatuses '2024-12-01 10:00:30.527'