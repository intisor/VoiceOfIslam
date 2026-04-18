CREATE TABLE public."AudioStreams" (
    "Id" uuid PRIMARY KEY,
    "Title" varchar(200) NOT NULL,
    "Description" varchar(500) NOT NULL,
    "BlobUrl" text NOT NULL,
    "CreatedAt" timestamptz NOT NULL,
    "ScheduledAt" timestamptz NULL,
    "IsLive" boolean NOT NULL,
    "Speaker" varchar(100) NOT NULL,
    "Duration" interval NOT NULL
);