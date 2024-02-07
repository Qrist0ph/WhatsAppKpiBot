
# Export Imager on Dev Machine
```
cd /mnt/d/repos/WhatsAppKpiBot/
```

```
docker build --pull -t whatsappkpibot .
```

```
cd /mnt/d/repos/dockerimages
```

```
docker save -o whatsappkpibot.tar whatsappkpibot
```

Now scp to server

# Import Image on server & run

 * kill older container
```bash
docker kill whatsappkpibot
```
 * then remove older container
```bash
docker rm  whatsappkpibot
```

 * now import the new image
```bash
docker load -i /root/repos/dockerimages/whatsappkpibot.tar
```
 * run it
```bash
docker run -d -p 5072:8080 -e whatsappphoneid=<ID> -e whatsapptoken=<TOKEN> --name whatsappkpibot whatsappkpibot
```
```bash
docker logs -f whatsappkpibot
```


[https://srvr1.yaico.de/whatsapp-kpi-bot/webhook](https://srvr1.yaico.de/whatsapp-kpi-bot/KpiBot/webhook)
