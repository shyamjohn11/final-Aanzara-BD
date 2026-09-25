@echo off
REM Rotate compromised credentials after removing them from git.
REM 1) JWT SigningKey  - generate 64+ random chars, set JWT__SigningKey
REM 2) SMTP AppPassword - create new Gmail App Password, set EMAIL__AppPassword
REM 3) SQL password     - alter SQL login, set ConnectionStrings__Default
REM 4) Purge git history of appsettings.json secrets (filter-repo / BFG)
REM 5) Force user logout by restarting API (invalidates old JWTs once key rotates)

setx JWT__SigningKey "REPLACE_WITH_NEW_64_CHAR_RANDOM_KEY"
setx EMAIL__AppPassword "REPLACE_WITH_NEW_GMAIL_APP_PASSWORD"
setx ConnectionStrings__Default "Server=...;Uid=...;Pwd=REPLACE;Database=...;TrustServerCertificate=True"

echo Update Seed:AdminEmail via SEED__AdminEmail if needed.
echo Restart the API after setting these. Do NOT commit values.
