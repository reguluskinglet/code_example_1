import 'whatwg-fetch';

function parseJSON(response) {
  return response.json();
}

export default function request(url, options, onError) {
  return fetch(url, options)
    .then((response) => {
      if (response.status === 503) {
        if (onError) {
          onError({
            "status": "503",
            "message": "Сервис недоступен",
          });
        }
        return null;
      }
      return response;
    })
    .then(parseJSON)
    .then((response) => {
      if (response && response.status !== null && response.status !== 'OK') {
        if (onError) {
          onError(response);
        }
        return null;
      }
      return response;
    });
}
