# Make Folder From File Name

A console application to create subfolders from files.

## Program Usage

```console
usage: [options...] [files or folders...]
options:
  /D:format             set output folder path format (default="<DIR>")
  /F:format             set output folder name format (default="<TITLE>")
  /G:format             set output file name format (default="<NAME>")
                        format special symbols:
                <CWD> - current working directory
                <DIR> - input file directory name
              <TITLE> - input file title
                <EXT> - input file extension
               <NAME> - input file name
                <LEN> - input file length
              <CTIME> - input file creation time
              <MTIME> - input file last write time
              <ATIME> - input file last access time
  /N:format             set number format, used with <LEN> (default="g")
  /T:format             set date/time format, used with <?TIME> (default="yyyy'-'MM'-'dd HH':'mm':'ss'Z'")
  /P:pattern            set file search pattern (default="*.*")
  /R                    enable recursive subfolder search (default=False)
  /M                    move input file to output folder (default=False)
  /C                    copy input file to output folder (default=False)
  /W                    overwrite existing files (default=False)
  /X                    sanitize output path and name (default=False)
  /S                    enable simulation mode (default=False)
  /V                    enable verbose mode (default=False)
  /Q                    enable quiet mode (default=False)
  /?                    show program usage
examples:
  1. create subfolders only
     "docs"
  2. create subfolders, moving files over, renaming them "folder"
     /C /R "/P:*.jpeg" "/G:folder<EXT>" "pics"
```
