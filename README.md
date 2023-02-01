# Slaveoftime

This is a blog project mainly for experimenting ideas. 

- You can write static posts in folder *Slaveoftime.Site/UI/Pages/Posts*
- You can write dynamic posts in folder *Slaveoftime.Site/UI/Pages/Posts*, but will require fsharp knowledge
- Support route post by id and title slug
- Use sqllite as backend database to record view count, likes and dislikes
- Use tailwindcss and daisyUI for style, so it is easy to customize

## How to use

    dotnet fsi build.fsx -- -h

    > require dotnet sdk 7
    