BEGIN TRANSACTION;

create table bg_job
(
id integer primary key,
[state] int,
build_data text,
created_on text,
start_on text,
completed_on text,
exception text,
lock text
);

COMMIT;