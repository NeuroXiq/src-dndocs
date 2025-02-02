BEGIN TRANSACTION;

create table djob_remote_service
(
id integer primary key,
instance_name text,
server_ip_address text,
server_port int,
alive boolean,
created_on text,
updated_on text
);

COMMIT;