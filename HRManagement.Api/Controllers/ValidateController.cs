using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.Contracts;
using CSharpFunctionalExtensions;
using FluentValidation;
using ValidationContext = FluentValidation.ValidationContext<object>;
using FluentValidation.AspNetCore;
using FluentValidation.Results;

namespace HRManagement.Api.Controllers
{
    public class ValidateController<TController> : ControllerBase
    {
        /// <summary>
        /// The validation helpers.
        /// </summary>
        private readonly IEnumerable<IValidator> validators;

        /// <summary>
        /// The logger
        /// </summary>
        private readonly ILogger<TController> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="validators"/> class.
        /// </summary>
        /// <param name="logger">The validation helpers.</param>
        /// <param name="logger">The logger.</param>
        public ValidateController(IEnumerable<IValidator> validators, ILogger<TController> logger)
        {
            this.validators = validators;
            this.logger = logger;
        }

        /// <summary>
        /// Validates and execute the request against the provided function.
        /// </summary>
        /// <typeparam name="T">the result.</typeparam>
        /// <param name="query">The request.</param>
        /// <param name="execute">The execute function.</param>
        /// <returns>The validation result.</returns>
        protected async Task<ActionResult<T>> ValidateAndExecute<T>(IBaseRequest query, Func<IBaseRequest, Task<Result<T>>> execute)
        {
            Contract.Assert(execute != null);

            logger.LogTrace("Validating the query : {query} ", query);

            var validationResult = GetValidationResult(query);

            if (validationResult != null && !validationResult.IsValid)
            {
                logger.LogTrace("Query has validation errors : {request} ", query);
                validationResult.AddToModelState(ModelState, null);
                return BadRequest(ModelState);
            }

            var result = await execute(query).ConfigureAwait(false);

            if (result.IsFailure)
            {
                logger.LogTrace("Query handler failed with error : {@error} ", result.Error);
                return BadRequest(result.Error);
            }

            if (result.Value == null)
            {
                logger.LogTrace("Query executed successfully and returned empty result");
                return Ok(null);
            }

            logger.LogTrace("Query executed successfully with return value {@value}", result.Value);
            return Ok(result.Value);
        }

        /// <summary>
        /// Validates and execute the request against the provided function.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="execute">The execute.</param>
        /// <returns>The result.</returns>
        protected async Task<ActionResult> ValidateAndExecute(IBaseRequest command, Func<IBaseRequest, Task<Result>> execute)
        {
            Contract.Assert(execute != null);

            logger.LogTrace("Validating the command : {request} ", command);
            var validationResult = GetValidationResult(command);

            if (validationResult != null && !validationResult.IsValid)
            {
                logger.LogTrace("Command has validation errors : {@Errors} ", validationResult.Errors);
                validationResult.AddToModelState(ModelState, null);
                return BadRequest(ModelState);
            }

            var result = await execute(command).ConfigureAwait(false);

            if (result.IsFailure)
            {
                logger.LogTrace("Command failed with error : {@error} ", result.Error);
                return BadRequest(result.Error);
            }

            logger.LogTrace("Command executed successfully");
            return Ok();
        }

        /// <summary>
        /// Validates the specified request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>Validation result</returns>
        protected Result Validate(IBaseRequest request)
        {
            logger.LogTrace("Validating the query : {query} ", request);
            var validationResult = GetValidationResult(request);
            if (validationResult == null || validationResult.IsValid)
            {
                return Result.Success();
            }

            validationResult.AddToModelState(ModelState, null);
            return Result.Failure("Validation errors found");
        }

        /// <summary>
        /// Gets the validation result.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>The validation result.</returns>
        private FluentValidation.Results.ValidationResult GetValidationResult(IBaseRequest request)
        {
            if (request == null)
            {
                return new FluentValidation.Results.ValidationResult(new[] { new ValidationFailure("Request", "Request cannot be null") });
            }

            var context = new ValidationContext(request);
            return validators
                .Where(x => x.CanValidateInstancesOfType(request.GetType()))
                   .Select(v => v.Validate(context))
                   .FirstOrDefault();
        }
    }
}
