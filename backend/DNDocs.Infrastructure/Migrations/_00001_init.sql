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
[exe_thread_id] text,
builds_project_id int
);

--9b056e7a-f126-41b6-8b61-fca210de466c
CREATE TABLE project
(
id integer primary key autoincrement,
project_name text,
url_prefix text,
[description] text,
githuburl text,
[state] integer,
[state_details] integer,
comment text,
created_on text,
last_modified_on text,
last_build_start_on text,
last_build_completed_on text,
last_build_error_log text,
bghealthcheck_httpget_datetime text,
bghealthcheck_httpget_status integer,
nupkg_autorebuild_last_datetime text,
git_md_repo_url text,
git_md_branch_name text,
git_md_relative_path_docs text,
git_md_relative_path_readme text,
git_docs_commit_hash text,
md_autorebuild text,
docfx_template text,
project_type int,
nugetorg_package_name text,
nugetorg_package_version text,
pv_git_tag text,
pv_project_versioning_id int,
ps_autorebuild bool
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

CREATE TABLE ref_user_project
(
id integer primary key autoincrement,
userid integer,
[project_id] integer
);

CREATE TABLE system_message
(
id integer primary key autoincrement,
[type] integer,
[level] integer,
title text,
[message] text,
[datetime] text,
[user_id] integer,
[project_id] integer,
[trace_bgjob_id] integer,
project_versioning_id int
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

-- project versions

create table project_versioning
(
id integer primary key autoincrement,
project_name text,
project_website_url text,
url_prefix text,
last_autoupgrade_at text,
last_autoupgrade_error text,
git_docs_repo_url text,
git_docs_branch_name text,
git_docs_relative_path text,
git_homepage_relative_path text,
[user_id] integer,
autoupgrade BOOLEAN
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
is_listed BOOLEAN,
project_id integer,
project_versioning_id integer
);

create table git_repo_store
(
id integer primary key autoincrement,
uuid text,
git_repo_url text,
created_on text,
last_modified_on text,
last_access_on text
);