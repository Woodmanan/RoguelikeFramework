#NOTE TO SELF - Need to run twice, with *.asset and *.prefab
#Also possibly .scene
$files = Get-ChildItem -Recurse -Filter "*.prefab"


$files.ForEach({
    $content = Get-Content -LiteralPath $_.FullName
    $replaced = $content -replace 'asm: Assembly-CSharp', 'asm: RoguelikeFramework'
    Set-Content -Path $_.FullName -Value $replaced
})