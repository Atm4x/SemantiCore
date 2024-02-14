from sentence_transformers import SentenceTransformer
model_name = 'sentence-transformers/LaBSE'
model = SentenceTransformer(model_name, cache_folder="./")