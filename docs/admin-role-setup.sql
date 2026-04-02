ALTER TABLE adminuser
    ADD COLUMN role_name VARCHAR(50) NULL AFTER PASSWORD;

UPDATE adminuser
SET role_name = 'SuperAdmin'
WHERE role_name IS NULL OR TRIM(role_name) = '';

ALTER TABLE adminuser
    MODIFY COLUMN role_name VARCHAR(50) NOT NULL DEFAULT 'Support';

CREATE INDEX IX_adminuser_role_name ON adminuser (role_name);