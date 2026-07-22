-- Migración: Agregar columna contacto_outlet a la tabla gesto
-- Ejecutar en la base de datos MySQL antes de iniciar el backend

ALTER TABLE `gesto`
ADD COLUMN `contacto_outlet` INT NULL DEFAULT NULL
AFTER `sk_aparato_id`;
