.\3rdparty\bgfx\.build\win64_vs2022\bin\shadercRelease.exe -f .\TritonSimRenderer\vertex.sc -o .\TritonSimRenderer\vertex.bin --type vertex --platform windows -p s_5_0 -i ".\3rdparty\bgfx\src" -i ".\TritonSimRenderer"


.\3rdparty\bgfx\.build\win64_vs2022\bin\shadercRelease.exe -f .\TritonSimRenderer\frag.sc -o .\TritonSimRenderer\frag.bin --type fragment --platform windows -p s_5_0 -i ".\3rdparty\bgfx\src" -i ".\TritonSimRenderer"