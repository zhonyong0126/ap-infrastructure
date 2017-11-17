# 该脚用于将指定的Project编译、打包并上传到MyGet上
import os
import sys
import subprocess
import re
import logging

logging.basicConfig(level=logging.DEBUG,
                    format='%(asctime)s - %(levelname)s - %(message)s')

def get_artifacts_dir(baseDir):
    """ Get the artifacts dir"""
    return os.path.join(baseDir, 'artifacts')

def get_src_dir(baseDir):
    """ Get the srouce code dir"""
    return os.path.join(baseDir,'src')

def scan_projects(baseDir):
    """ Sacn all projects """
    projects = []
    srcDir=get_src_dir(baseDir)
    files = os.listdir(srcDir)
    os.chdir(get_src_dir(baseDir))
    for f in files:
        if os.path.isdir(os.path.join(srcDir, f)):
            projects.append(f)
    lenOfProjs = len(projects)
    if lenOfProjs == 0:
        return None
    return projects


def chose_project(projects):
    """ List all projects and asks for you choose"""

    lenOfProjs = len(projects)

    # Shows all projects
    i = 0
    while i < lenOfProjs:
        print(str(i + 1) + ': ' + projects[i])
        i += 1

    while True:
        # choseSeq = 4
        choseSeq = input('Which project is you want? ')
        if choseSeq == 'exit':
            return None

        try:
            seq = int(choseSeq)
        except ValueError:
            print('Invalid input')
            continue

        if seq < 1 or seq > lenOfProjs:
            print('You inputted is invalid.')
            continue

        return projects[seq - 1]


def make_project_file(basedir, proj):
    return os.path.join(basedir,get_src_dir(cwd), proj, proj + '.csproj')


def get_latest_version(proj):
    """ Get latest version of nupkg"""
    rex = proj.replace('.', '\.') + r'\.(\d*\.\d*.\d*)\.nupkg'
    logging.debug('The nupkg regex is ' + rex)
    nupkgRex = re.compile(rex)
    artifactsDir = get_artifacts_dir(cwd)
    files = os.listdir(artifactsDir)
    versions = []
    for f in files:
        mo = nupkgRex.search(f)
        if mo is None:
            continue
        versions.append(mo.group(1))

    lenOfVersions = len(versions)
    if lenOfVersions == 0:
        print('No existing version found.')
        return None
    versions.sort(reverse=True)
    return versions[0]


def do_work(label, command):
    print(label)
    retcode = subprocess.call(command, shell=True)
    if retcode == 0:
        print('Complete')
    return retcode


def build_pack_project(baseDir, proj, version):
    artifactsDir=get_artifacts_dir(baseDir)

    retcode = do_work('Restoring...', 'dotnet restore ' +
                      proj + ' 1>/dev/null')
    if retcode != 0:
        return

    # retcode = do_work('Building...', 'dotnet build ' + proj + ' 1>/dev/null')
    # if retcode != 0:
    #     return

    retcode = do_work(
        'Packing...', 'dotnet pack -c Release --include-symbols --version-suffix ' + version + ' -o ' + artifactsDir + ' ' + proj + ' 1>/dev/null')
    if retcode != 0:
        return

    return 0

def upload_nupkg(nupkgFile):
    retcode=do_work('Uploading...','dotnet nuget push ' + nupkgFile + ' --api-key f86deb23-2448-486a-a973-5880a07915a0 --source  https://www.myget.org/F/roc-ap/api/v2/package' )
    if retcode!=0:
        return None
    return 0

# 获取并打印当前目录
cwd = os.getcwd()
print('Pls note that current directory is ' + cwd)

# 获取当前目录中所有的Projects
projects = scan_projects(cwd)
if projects is None:
    print('No project detected.')
    sys.exit(1)

# 提示选择哪个project
choseProj = chose_project(projects)
if choseProj is None:
    print('Bye Bye')
    sys.exit(0)

print('You chose the ' + choseProj)

latestVer = get_latest_version(choseProj)
if latestVer is None:
    newVerMsg = 'There is no existing nupkg.'
else:
    newVerMsg = 'The latest version is ' + latestVer + '.'
newVer = input(newVerMsg + ' Pls type new version number: ')

projfile = make_project_file(cwd, choseProj)
build_pack_project(cwd, projfile, newVer)
upload_nupkg(os.path.join(get_artifacts_dir(cwd),choseProj + '.' + newVer + '.nupkg'))

sys.exit(0)
