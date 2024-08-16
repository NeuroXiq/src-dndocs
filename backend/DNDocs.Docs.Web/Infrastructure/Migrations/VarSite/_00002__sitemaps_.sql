BEGIN TRANSACTION;

create table sitemap
(
id integer primary key,
[path] text,
decompressed_length int,
urls_count int,
updated_on text,
byte_data BLOB
);

delete from public_html where [path] like '/sitemap%';

drop table sitemap_project;

create table sitemap_project
(
id integer primary key,
sitemap_id int,
project_id int
);

COMMIT;