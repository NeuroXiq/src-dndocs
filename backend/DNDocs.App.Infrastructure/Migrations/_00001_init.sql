PRAGMA journal_mode=WAL;

CREATE TABLE [app_setting]
(
id integer primary key autoincrement,
[key] text,
[value] text
);

CREATE TABLE [user]
(
id integer primary key autoincrement,
[login] text,
primary_email text,
github_primary_email text,
github_id text,
github_login text,
github_repos_url text,
github_url text,
github_html_url text,
github_avatar_url text,
github_type text,
created_on text,
last_modified_on text
);

CREATE TABLE bgjob
(
id integer primary key autoincrement,
queued_datetime text,
started_datetime text,
completed_datetime text,
[status] int,
dowork_command_type text,
dowork_command_data text,
[execute_as_user_id] integer,
[command_handler_success] BOOLEAN,
[command_handler_result] text,
[exception] text,
[exe_thread_id] text
);

CREATE TABLE oauth_access_token
(
id integer primary key autoincrement,
[user_id] integer,
access_token text,
scope text,
token_type text,
createdon text
);

CREATE TABLE cache
(
id integer primary key autoincrement,
[created_on] text,
[last_modified_on] text,
[expiration] text,
[key] text,
[data] BLOB
);


CREATE TABLE nuget_package
(
id integer primary key autoincrement,
title text,
identity_version text,
identity_id text,
published_date text,
project_url text,
package_details_url text,
is_listed BOOLEAN
);

create table nugetorg_project
(
id integer primary key autoincrement,
package_name text,
package_version text,
is_online bool,
[state] int,
build_starton text,
created_on text,
last_modified_on text,
nuget_package_id int
);