from sentence_transformers import SentenceTransformer
from sklearn.metrics.pairwise import cosine_similarity
import socket
import json
import select
import sys

def main():
    if len(sys.argv) < 2:
        return
    print(sys.argv[1])

    server_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    server_socket.bind(('127.0.0.1', 19200))
    server_socket.listen(5)
    print(f"Сервер запущен на порту 19200.")
    client_socket, client_address = server_socket.accept()

    try:
        model_name = 'sentence-transformers/LaBSE'
        model = SentenceTransformer(model_name, cache_folder=f"{sys.argv[1]}")
        load = model.encode("loading script")
        client_socket.send(b'Loaded\n')
    except:
        client_socket.send(b'Error\n')
        exit()
    print(f"Подключение с {client_address} установлено.")

    print(f'Загружена модель {model_name}.')

    text1 = "Это первый документ, содержащий определенную информацию."
    #text2 = "Это второй документ, который несет схожую информацию."

    def index_file(filename):
        with open(filename, 'r', encoding='utf-8') as f:
            text = f.read()
            text_embedding = model.encode(text)
            return text_embedding


    def process_query():
        client_socket.setblocking(False)
        while 1 > 0:
            ready = select.select([client_socket], [], [], 1)
            if ready[0]:
                string = client_socket.recv(99999)
                if not string:
                    continue
                string = string.decode('utf-8')
                json_data = json.loads(string)

                print(string)
                if json_data['Type'] == 'IndexingFile':
                    print(f"Получен запрос на индексирование файла {json_data['Type']}")
                    embeddings = index_file(str(json_data['Value']))
                    client_socket.send(f"{json.dumps(embeddings.tolist())}\n".encode('utf-8'))


    process_query()

    def get_similarity(embedding1, embedding2):
        similarity = cosine_similarity([embedding1], [embedding2])[0][0]
        return similarity


main()
#text1 = open("C:\\Users\\dan19\\Desktop\\test1.txt", "r", encoding="utf-8").read()
