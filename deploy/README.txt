Put the MonsterASP.NET .publishSettings file here.
Put r2-credentials.json here too, containing AccountId, AccessKeyId, SecretAccessKey and BucketName for Cloudflare R2 (resume storage). Optional: without it, publish.ps1 still deploys, resume upload just returns a clear failure until the file exists.
This folder is gitignored: it contains a deployment password and must never be committed.
