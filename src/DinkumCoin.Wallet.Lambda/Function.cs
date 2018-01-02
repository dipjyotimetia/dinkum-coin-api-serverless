using System;
using Amazon.Lambda.Core;
using Amazon;
using Amazon.Lambda.APIGatewayEvents;
using StructureMap;
using System.Net;
using Newtonsoft.Json;
using System.Threading.Tasks;
using DinkumCoin.Wallet.Lambda.Services;
using DinkumCoin.Wallet.Lambda.Contracts;
using System.ComponentModel.DataAnnotations;
using DinkumCoin.Core.Contracts;
using DinkumCoin.Data.Repositories;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]
namespace DinkumCoin.Wallet.Lambda
{
    
    public class Function
    {
        private IContainer _container = null;

        public Function(IContainer container)
        {
            _container = container;
        }

        public Function()
        {
            _container = CreateContainer();
        }


        public async Task<APIGatewayProxyResponse> GetWallets(APIGatewayProxyRequest request, ILambdaContext context)
        {
            try
            {
                var repo = _container.GetInstance<IDinkumRepository>();
                var allWallets = await repo.GetAllWallets();

                var response = new APIGatewayProxyResponse
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Body = JsonConvert.SerializeObject(allWallets)
                };
                return response;
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        public async Task<APIGatewayProxyResponse> GetWallet(APIGatewayProxyRequest request, ILambdaContext context)
        {
            var walletId = request.PathParameters["walletId"];
            try
            {
                var repo = _container.GetInstance<IDinkumRepository>();
                var wallet = await repo.GetWallet(new Guid(walletId));

                var response = new APIGatewayProxyResponse
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Body = JsonConvert.SerializeObject(wallet)
                };
                return response;
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        public async Task<APIGatewayProxyResponse> MineCoin(APIGatewayProxyRequest request, ILambdaContext context)
        {
            var walletId = request.PathParameters["walletId"];
            try
            {
                var repo = _container.GetInstance<IDinkumRepository>();
                var miningService = _container.GetInstance<IMiningService>();

                var mineResult = miningService.AttemptMineNewCoin();

                if (mineResult.CoinCreated)
                {
                    var wallet = await repo.AddCoinToWallet(new Guid(walletId), mineResult.NewCoin);

                    var response = new APIGatewayProxyResponse
                    {
                        StatusCode = (int)HttpStatusCode.Created,
                        Body = JsonConvert.SerializeObject(wallet)
                    };
                    return response;
                }
                else
                {
                    var response = new APIGatewayProxyResponse
                    {
                        StatusCode = (int)HttpStatusCode.NotModified,
                        Body = "Mining Attempt unsuccesful"
                    };
                    return response;
                }


            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        private APIGatewayProxyResponse BadRequest(string message)
        {
            var response = new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Body = message
            };
            return response;
        }

        private APIGatewayProxyResponse NotFound(string message)
        {
            var response = new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.NotFound,
                Body = message
            };
            return response;
        }






        public IContainer CreateContainer()
        {
            try
            {
                var container = new Container();
                container.Configure(config =>
                {
                    config.For<IMathService>().Use<MathService>();
                    config.For<IMiningService>().Use<MiningService>();
                    config.For<IDinkumRepository>().Use<DynamoRepository>();

                });
                return container;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


    }



}
