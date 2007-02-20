INSERT INTO linqtestdb.alltypes (
               `intN` ,
  `double` ,   `doubleN` ,
  `decimal` ,  `decimalN` ,
  `blob` ,     `blobN` ,
  `boolean` ,  `boolN` ,
  `byte` ,     `byteN` ,
  `DateTime` , `DateTimeN` ,
  `float` ,    `floatN` ,
  `char` ,     `charN` ,
  `text` ,     `textN` ,
  `short` ,    `shortN` ,
  `numeric` ,  `numericN` ,
  `real` ,     `realN` ,
  `smallInt` , `smallIntN`
)
VALUES(         null,
  2,            null, /*double*/
  3,            null,
  'aa',         null, /*blob*/
  true,         null,
  8,            null, /*byte*/
  now(),        null,
  4,            null, /*float*/
  'c',          null,
  'text',       null, /*text*/
  127,          null,
  999.9,        null, /*numeric*/
  998.9,        null,
  16000,        null);
