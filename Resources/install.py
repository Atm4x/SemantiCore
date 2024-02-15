from sentence_transformers import SentenceTransformer
import sys

def main():
    if len(sys.argv) < 2:
        return
    print(sys.argv[1])
    model_name = 'sentence-transformers/LaBSE'
    model = SentenceTransformer(model_name, cache_folder=f"{sys.argv[1]}")

main()