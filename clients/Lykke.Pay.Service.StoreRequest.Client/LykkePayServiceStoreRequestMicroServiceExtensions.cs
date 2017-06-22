// Code generated by Microsoft (R) AutoRest Code Generator 1.1.0.0
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.

namespace Lykke.Pay.Service.StoreRequest.Client
{
    using Lykke.Pay;
    using Lykke.Pay.Service;
    using Lykke.Pay.Service.StoreRequest;
    using Models;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Extension methods for LykkePayServiceStoreRequestMicroService.
    /// </summary>
    public static partial class LykkePayServiceStoreRequestMicroServiceExtensions
    {
            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='request'>
            /// </param>
            public static void ApiStorePost(this ILykkePayServiceStoreRequestMicroService operations, MerchantPayRequest request = default(MerchantPayRequest))
            {
                operations.ApiStorePostAsync(request).GetAwaiter().GetResult();
            }

            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='request'>
            /// </param>
            /// <param name='cancellationToken'>
            /// The cancellation token.
            /// </param>
            public static async Task ApiStorePostAsync(this ILykkePayServiceStoreRequestMicroService operations, MerchantPayRequest request = default(MerchantPayRequest), CancellationToken cancellationToken = default(CancellationToken))
            {
                (await operations.ApiStorePostWithHttpMessagesAsync(request, null, cancellationToken).ConfigureAwait(false)).Dispose();
            }

    }
}
