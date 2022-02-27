# Slaveoftime

This project will pull a public github repos and find any .md files which contains below data at the begining:


``` txt
- id: 7e494852-ecc9-477d-910c-b1a4d31a75bd
- title: Demo title
- keywords: key1,key2
- description: some description
- createTime: 2022-02-20
---
```

    id, title is required
    assets must be put in the same folder for the related md file


## How to develop

    - cd to ./Slaveoftime.Site

    - pnpm install
    - pnpm run watch-css

    - open a new terminal and run:
    - dotnet run

    - open a new terminal and run:
    - fun-blazor watch .\Slaveoftime.Site.fsproj -s https://localhost:6001

