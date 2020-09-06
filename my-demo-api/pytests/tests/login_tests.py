import urllib3
import json
import requests
import urllib3
import logging
import os
import pytest
import time
from signalrcore.hub_connection_builder import HubConnectionBuilder

ENV = os.environ.get("CI_COMMIT_REF_SLUG")
#url = "https://$ENV.demo-cti-service.stage.demo.ru"
url = "http://localhost:3451"

hub_connection = HubConnectionBuilder()\
    .with_url("ws://localhost:3451/phoneHub")\
    .configure_logging(logging.DEBUG)\
    .with_automatic_reconnect({
        "type": "raw",
        "keep_alive_interval": 5,
        "reconnect_interval": 2,
        "max_attempts": 1
    })\
    .build()
    
pytest.selectedUser = ""
pytest.userConnected = False

def disable_warnings():
    urllib3.disable_warnings(urllib3.exceptions.InsecureRequestWarning)

def on_active_operators(data):
    pytest.userConnected = True

@pytest.fixture(scope="session", autouse=True)
def start(request):
    print("---START LOGIN TESTS---")
    hub_connection.on_open(lambda: print("connection opened and handshake received ready to send messages"))
    hub_connection.on("activeOperators", on_active_operators)
    hub_connection.start()
    time.sleep(2)

@pytest.fixture(autouse=True)
def run_around_tests():
    disable_warnings()
    yield
    # Code that will run after your test

class TestApi:
    LOGGER = logging.getLogger(__name__)

    def test_get_operators_list(self):
        r = requests.get(url + "/api/operators/nonactive", verify=False, headers={"Content-Type": "application/json"})
        if r.status_code == 200 and len(r.json()) != 0:
            pytest.selectedUser = r.json()[0]
            pass
        else:
            raise AssertionError and print("test_get_operators_list. Error loading operators")

    def test_post_login_with_extension(self):
        m = {'extension': pytest.selectedUser["extension"]}
        r = requests.post(url + "/api/operators", verify=False, data=json.dumps(m), headers={"Content-Type": "application/json"})
        assert r.status_code == 200
        responseUser = r.json()
        if 'userName' in responseUser and responseUser['userName'] == pytest.selectedUser["userName"]:
            pytest.selectedUser = responseUser
            pass
        else:
            raise AssertionError and print('Error getting user to login')
            
    def test_get_operator_info(self):
        r = requests.get(url + "/api/operators/" + pytest.selectedUser["id"], verify=False, headers={"Content-Type": "application/json"})
        if r.status_code == 200 and r.json()['userName'] == pytest.selectedUser["userName"]:
            pytest.selectedUser = r.json()
            pass
        else:
            raise AssertionError and print("test_get_operator. Error getting operator by id")
        TestApi.LOGGER.info(r.status_code)

    def test_sirnalr_operator_connect(self):
        hub_connection.send("OperatorConnect", [pytest.selectedUser])
        time.sleep(2)
        if pytest.userConnected == True:
            pass
        else:
            raise AssertionError and print('User connection failed')

    def test_get_active_operators_list(self):
        r = requests.get(url + "/api/operators/active", verify=False, headers={"Content-Type": "application/json"})
        operators = r.json()
        print("operatorsCount", len(operators), r.status_code)
        if pytest.userConnected and r.status_code == 200 and len(operators) == 1 and pytest.selectedUser["id"] == operators[0]["id"]:
            pass
        else:
            raise AssertionError and print("test_get_active_operators_list. Error loading active operators")
