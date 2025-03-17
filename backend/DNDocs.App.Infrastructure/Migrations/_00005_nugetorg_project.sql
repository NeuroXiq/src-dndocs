BEGIN TRANSACTION;

create table nugetorg_project
(
id integer primary key,
package_name text,
package_version text,
is_online bool,
[state] int,
build_starton text,
created_on text,
last_modified_on text
);

COMMIT;