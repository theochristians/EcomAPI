using EComAPI.Application.Common.Result;
using EComAPI.Application.Transaction.DTOs;
using EComAPI.Application.Transaction.Interfaces;
using EComAPI.Domain.Common.Exceptions;

namespace EComAPI.Application.Transaction.Queries.StockLogQueries.GetStockLogs
{
    public class GetStockLogsHandler
    {
        private readonly IStockLogRepository _stockLogRepository;

        public GetStockLogsHandler(IStockLogRepository stockLogRepository)
        {
            _stockLogRepository = stockLogRepository;
        }

        public async Task<Result<IReadOnlyList<StockLogDto>>> Handle(
            GetStockLogsQuery query,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var logs = await _stockLogRepository.GetLogsByVariantIdAsync(
                    query.ProductVariantId, cancellationToken);

                var logDtos = logs
                    .Select(stockLog => new StockLogDto(
                        stockLog.Id,
                        stockLog.ProductVariantId,
                        stockLog.Type,
                        stockLog.QuantityChange,
                        stockLog.StockBefore,
                        stockLog.StockAfter,
                        stockLog.ReferenceType,
                        stockLog.ReferenceId,
                        stockLog.Note,
                        stockLog.CreatedAt,
                        stockLog.CreatedBy))
                    .ToList();

                return Result<IReadOnlyList<StockLogDto>>.Success(logDtos);
            }
            catch (DomainException domainException)
            {
                return Result<IReadOnlyList<StockLogDto>>.Failure(domainException.Message);
            }
            catch (Exception)
            {
                return Result<IReadOnlyList<StockLogDto>>.Failure("An error occurred while retrieving stock logs");
            }
        }
    }
}
