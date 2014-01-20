@echo off
packages\OpenCover.4.5.2316\OpenCover.Console.exe -register:user -target:.\packages\NUnit.runners.2.6.3\tools\nunit-console-x86.exe -targetargs:"tests\unit-tests.nunit /noshadow" -filter:"+[*]* -[*]*.Migrations.* -[*]Internal.*" -output:testCoverage.xml

REM Generate the HTML version of the coverage report
packages\ReportGenerator.1.9.1.0\ReportGenerator.exe -reports:"testCoverage.xml" "-targetdir:coverage"

REM Display the report
start coverage\index.htm