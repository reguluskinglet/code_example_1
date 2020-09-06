# Для установки всего необходимого требуется вызвать следующие команды:

# установка питона версии 3.6 и выше
sudo apt install python3

#установка пакетного менеджера pip
sudo apt install python3-pip

#установка requirements.txt
pip3 install -r requirements.txt

#после завершения установок требуется запустить тесты
pytest /path/to/tests/login_tests.py
