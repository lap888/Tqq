#!/bin/bash
echo ">>进入打包项目路径"
cd /Users/topbrids/qq/Tqq
echo ">>查看当前路径下文件"
ls
echo ">>执行打包发布命令"
dotnet publish -c release -o /Users/topbrids/qq/Tqq/relese
echo ">>进入到打包好的路径查看"
cd /Users/topbrids/qq/Tqq/relese
ls
echo ">>打成压缩包"
tar -zcvf relese.tar.gz relese
echo ">>上传到服务器 请输入密码:"
scp -i /Users/topbrids/cert/LFEX.pem -P 6001 relese/relese.tar.gz root@49.233.134.249:/apps/project/hwhj/
echo ">>证书远端连接服务器 请输入密码:"
ssh -p 6001 -i /Users/topbrids/cert/LFEX.pem root@49.233.134.249