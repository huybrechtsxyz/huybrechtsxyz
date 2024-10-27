using Microsoft.Extensions.Configuration;
using Minio;
using Minio.DataModel.Args;
using Piranha;
using Piranha.Models;

namespace PiranhaMinioApp.Storage
{
    public class MinioStorage : IStorage
    {
        private readonly MinioClient _minioClient;
        private readonly string _bucketName;

        public MinioStorage(IConfiguration configuration)
        {
            var endpoint = configuration["MinioSettings:Endpoint"];
            var accessKey = configuration["MinioSettings:AccessKey"];
            var secretKey = configuration["MinioSettings:SecretKey"];
            var useSSL = bool.Parse(configuration["MinioSettings:UseSSL"] ?? "true");
            _bucketName = configuration["MinioSettings:Bucket"];

            _minioClient = (MinioClient?)new MinioClient()
                .WithEndpoint(endpoint)
                .WithCredentials(accessKey, secretKey)
                .WithSSL(useSSL)
                .Build();

            // Ensure the bucket exists
            EnsureBucketExists().Wait();
        }

        private async Task EnsureBucketExists()
        {
            bool found = await _minioClient.BucketExistsAsync(new BucketExistsArgs().WithBucket(_bucketName));
            if (!found)
            {
                await _minioClient.MakeBucketAsync(new MakeBucketArgs().WithBucket(_bucketName));
            }
        }

        public async Task PutAsync(string id, string contentType, Stream stream)
        {
            await _minioClient.PutObjectAsync(new PutObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(id)
                .WithStreamData(stream)
                .WithObjectSize(stream.Length)
                .WithContentType(contentType));
        }

        public async Task<Stream> GetAsync(string id)
        {
            var memoryStream = new MemoryStream();
            await _minioClient.GetObjectAsync(new GetObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(id)
                .WithCallbackStream((stream) => stream.CopyTo(memoryStream)));
            memoryStream.Position = 0;
            return memoryStream;
        }

        public async Task DeleteAsync(string id)
        {
            await _minioClient.RemoveObjectAsync(new RemoveObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(id));
        }

        public Task<IStorageSession> OpenAsync()
        {
            throw new NotImplementedException();
        }

        public string GetPublicUrl(Media media, string filename)
        {
            throw new NotImplementedException();
        }

        public string GetResourceName(Media media, string filename)
        {
            throw new NotImplementedException();
        }
    }
}
