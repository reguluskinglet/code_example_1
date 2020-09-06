# Прототип сервиса proto-cti-service

Прототип сервиса, отвечающего за обработку вызовов от заявителей; взаимодействие с телефонией.

## Дополнительная информация
[CTI] (https://wiki.merionet.ru/ip-telephoniya/6/computer-telephony-integration/)

## Redis

Redis можно поднять с помощью `docker-compose up -d redis`

Для проверки работоспособности Redis из командной строки:
  * Install Redis CLI: `apt-get install redis-tools`
  * Выполнить команду: `redis-cli ping`, в ответ должно придти `PONG`