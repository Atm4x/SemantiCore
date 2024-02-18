from sentence_transformers import SentenceTransformer
import socket
import json
import select
import sys
import win32com.client
import pypdfium2 as pdfium
import textwrap as tw

exts_docs = ['.docx', '.doc', '.docm', '.dotx', '.dot', '.dotm', '.rtf']
exts_pdf = ['.pdf']
exts_txt = ['.txt']

encodings = ['utf-8', 'ansi', 'utf-8-sig', 'utf-16', 'utf-32', 'cp1252', 'cp1250', 'cp1251', 'latin-1', 'ascii', 'iso-8859-1', 'utf-32']

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

    def ends_for(string, exts):
        for ext in exts:
            if string.endswith(ext):
                return True
        return False

    def check_for_encoding(filename, encoding):
        try:
            with open(filename, 'r', encoding=encoding) as f:
                text = f.read()
                return True, text
        except UnicodeDecodeError:
            return False, ""

    def index_file(filename):

        if ends_for(filename, exts_txt):
            for encoding in encodings:
                checked, text = check_for_encoding(filename, encoding)
                if not checked:
                    continue

                text_embeddings = []
                tw_text = tw.wrap(text, 10000, break_long_words=False)
                for piece in tw_text:
                    text_embeddings.append(model.encode(piece).tolist())
                return text_embeddings

        elif ends_for(filename, exts_docs):
            doc = win32com.client.GetObject(filename)
            text = doc.Content.Text
            text1 = doc.Range().Text
            print(text == text1)
            text_embeddings = []
            tw_text = tw.wrap(text, 10000, break_long_words=False)
            for piece in tw_text:
                text_embeddings.append(model.encode(piece).tolist())
            return text_embeddings

        elif ends_for(filename, exts_pdf):
            pdf = pdfium.PdfDocument(filename)
            text = []
            text_embeddings = []
            for page in pdf:
                text.append(page.get_textpage().get_text_range() + '\n')
            tw_text = tw.wrap('\n'.join(text), 10000, break_long_words=False)
            for piece in tw_text:
                text_embeddings.append(model.encode(piece).tolist())
            return text_embeddings

    def process_query():
        client_socket.setblocking(False)
        while 1 > 0:
            ready = select.select([client_socket], [], [], 1)
            if ready[0]:
                try:
                    data = socket.SocketIO(sock=client_socket, mode='r').readline()
                    if not data:
                        break

                    json_data = json.loads(data.decode('utf-8'))

                    if json_data['Type'] == 'IndexingFile':
                        print(f"Получен запрос на индексирование файла {json_data['Type']}")
                        embeddings = index_file(str(json_data['Value']))
                        client_socket.send(f"{json.dumps(embeddings)}\n".encode('utf-8'))
                    elif json_data['Type'] == 'IndexingText':
                        print(f"Получен запрос на индексирование текста {json_data['Type']}")
                        embeddings = model.encode(str(json_data['Value']))
                        client_socket.send(f"{json.dumps(embeddings.tolist())}\n".encode('utf-8'))
                except:
                    print(f"Ошибка при получении данных")
                    break

    process_query()


main()
