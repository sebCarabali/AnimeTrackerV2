import { ArgumentsHost, Catch, ExceptionFilter } from '@nestjs/common';
import type { Response } from 'express';
import { CoreUnavailableException } from './core-unavailable.exception';

/**
 * Nest's ExpressAdapter.reply() reverts Content-Type to application/json for any object
 * body paired with a >=400 status, so an application/problem+json response must be sent
 * through the raw Express response (bypassing reply()) rather than a normal handler return.
 * A @Catch() filter is the sanctioned extension point for this: unlike @Res() in the
 * controller, the exception still flows through Nest's interceptor/guard pipeline up to
 * this point, so future interceptors (e.g. correlation/tracing for HU-00.11) can still
 * observe it via RxJS catchError/tap on the handler's observable.
 */
@Catch(CoreUnavailableException)
export class ProblemJsonExceptionFilter implements ExceptionFilter {
  catch(exception: CoreUnavailableException, host: ArgumentsHost): void {
    const response = host.switchToHttp().getResponse<Response>();
    response
      .status(exception.getStatus())
      .setHeader('Content-Type', 'application/problem+json')
      .json(exception.getResponse());
  }
}
