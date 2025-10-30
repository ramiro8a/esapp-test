namespace PaymentService.API.Controllers;
using Microsoft.AspNetCore.Mvc;
using PaymentService.Application.DTOs;
using PaymentService.Application.Services;
using Swashbuckle.AspNetCore.Annotations;

[ApiController]
[Route("api/v1/payments")]
[Produces("application/json")]
[Consumes("application/json")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(IPaymentService paymentService, ILogger<PaymentsController> logger)
    {
        _paymentService = paymentService;
        _logger = logger;
    }

    [HttpPost]
    [SwaggerOperation(
        Summary = "Crear nuevo pago",
        Description = "Crea un nuevo pago con estado 'evaluating' y lo envía automáticamente al servicio de evaluación de riesgos. " +
                      "El servicio espera la respuesta asincronamente para actualizar el estado del pago a 'accepted' o 'denied'.")]
    [SwaggerResponse(201, "Pago creado exitosamente", typeof(PaymentResponse))]
    [SwaggerResponse(400, "Error en los datos proporcionados o en la solicitud")]
    [SwaggerResponse(500, "Error interno del servidor")]
    public async Task<ActionResult<PaymentResponse>> CreatePayment(
        [FromBody]
        [SwaggerParameter("Datos requeridos para crear el pago")]
        CreatePaymentRequest request)
    {
        try
        {
            _logger.LogInformation("Creando nuevo pago para cliente: {CustomerId}", request.CustomerId);
            var response = await _paymentService.CreatePaymentAsync(request);
            return CreatedAtAction(nameof(GetPayment), new { externalOperationId = response.ExternalOperationId }, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creando pago para cliente: {CustomerId}", request.CustomerId);
            return BadRequest(new { error = "Error al procesar la solicitud de pago", details = ex.Message });
        }
    }

    [HttpGet("{externalOperationId:guid}")]
    [SwaggerOperation(
        Summary = "Consultar pago por ID",
        Description = "Obtiene el estado actual del pago incluyendo si fue aceptado o rechazado. " +
                      "Estados posibles: 'evaluating' (en evaluación), 'accepted' (aceptado), 'denied' (rechazado).")]
    [SwaggerResponse(200, "Pago encontrado con sus detalles", typeof(PaymentResponse))]
    [SwaggerResponse(404, "Pago no encontrado con ese ID")]
    [SwaggerResponse(400, "Error en la solicitud")]
    public async Task<ActionResult<PaymentResponse>> GetPayment(
        [FromRoute]
        [SwaggerParameter("ID único de la operación de pago (GUID)")]
        Guid externalOperationId)
    {
        try
        {
            _logger.LogInformation("Obteniendo pago con ID: {ExternalOperationId}", externalOperationId);
            var response = await _paymentService.GetPaymentAsync(externalOperationId);
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Pago no encontrado: {ExternalOperationId}", externalOperationId);
            return NotFound(new { error = "Pago no encontrado", externalOperationId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo pago: {ExternalOperationId}", externalOperationId);
            return BadRequest(new { error = "Error al consultar el pago", details = ex.Message });
        }
    }
}
