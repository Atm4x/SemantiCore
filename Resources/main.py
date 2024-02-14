from sentence_transformers import SentenceTransformer
from sklearn.metrics.pairwise import cosine_similarity
import socket

server_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
server_socket.bind(('127.0.0.1', 19499))
server_socket.listen(5)

print(f"Сервер запущен на порту 19499.")
client_socket, client_address = server_socket.accept()

print(f"Подключение с {client_address} установлено.")
try:
    model_name = 'sentence-transformers/LaBSE'
    model = SentenceTransformer(model_name)
    client_socket.send(b'Loaded\n')
except:
    client_socket.send(b'Error\n')
    exit()


print(f'Загружено модель {model_name}.')

text1 = "Это первый документ, содержащий определенную информацию."
#text2 = "Это второй документ, который несет схожую информацию."

#text1 = open("C:\\Users\\dan19\\Desktop\\test1.txt", "r", encoding="utf-8").read()
print(text1)
embedding1 = model.encode(text1)




#print(cosine_similarity([embedding1], [embedding2]))