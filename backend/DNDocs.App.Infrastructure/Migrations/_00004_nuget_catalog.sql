BEGIN TRANSACTION;

create table nuget_catalog_page
(
id integer primary key,
nid text,
commit_id text,
commit_timestamp text,
[count] int,
[state] int,
created_on text,
last_modified_on text
);

COMMIT;