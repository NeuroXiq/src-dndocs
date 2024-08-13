PRAGMA journal_mode=WAL;

BEGIN TRANSACTION;

create table project
(
	id integer primary key autoincrement,
	dn_project_id integer,
	metadata text,
	url_prefix text,
	project_version text,
	nuget_package_name text,
	nuget_package_version text,
	project_type int,
	created_on text,
	updated_on text
);

create table system_state
(
	id integer primary key autoincrement,
	[name] text,
	[value] text
);

COMMIT;