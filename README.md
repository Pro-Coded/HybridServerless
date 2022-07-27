# HybridServerless

[![Codacy Badge](https://app.codacy.com/project/badge/Grade/72c35f2824684eacb0bb75a3e3a80dad)](https://www.codacy.com/gh/Pro-Coded/pro-hybrid-serverless/dashboard?utm_source=github.com&amp;utm_medium=referral&amp;utm_content=Pro-Coded/pro-hybrid-serverless&amp;utm_campaign=Badge_Grade)
![example workflow](https://github.com/Pro-Coded/pro-hybrid-serverless/actions/workflows/dotnet.yml/badge.svg)

To create the Azure Infrastructure:
~~~~ csharp
cd HybridServerless.Functions

dotnet publish -c release

cd ../HybridServerless.IAC

pulumi up
~~~~