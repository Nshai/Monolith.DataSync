using System;
using System.ServiceModel;
using IntelliFlo.Platform.Caching;
using log4net;

namespace Microservice.Workflow.Engine
{
    public static class ServiceClient
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(ServiceClient));

        public static ServiceResult Call<T>(this T client, Action<T> action) where T : ICommunicationObject
        {
            var result = new ServiceResult();
            try
            {
                action(client);
                result.Success = true;
            }
            catch (CommunicationException ex)
            {
                logger.Error("Service communication exception", ex);
            }
            catch (TimeoutException ex)
            {
                logger.Error("Service timeout exception", ex);
            }
            catch (Exception ex)
            {
                logger.Error("Exception occurred calling service", ex);
                throw;
            }
            finally
            {
                CloseOrAbortServiceChannel(client);
            }
            return result;
        }

        public static ServiceResult<TResult> Call<T, TResult>(this T client, Func<T, TResult> action) 
            where T : ICommunicationObject
            where TResult : new()

        {
            var serviceResult = new ServiceResult<TResult>();
            try
            {
                var result = action(client);
                serviceResult.Result = result;
                serviceResult.Success = true;
            }
            catch (CommunicationException ex)
            {
                logger.Error("Service communication exception", ex);
            }
            catch (TimeoutException ex)
            {
                logger.Error("Service timeout exception", ex);
            }
            catch (Exception ex)
            {
                logger.Error("Exception occurred calling service", ex);
                throw;
            }
            finally
            {
                CloseOrAbortServiceChannel(client);
            }
            return serviceResult;
        }

        private static void CloseOrAbortServiceChannel(ICommunicationObject communicationObject)
        {
            bool isClosed = false;

            if (communicationObject == null || communicationObject.State == CommunicationState.Closed)
            {
                return;
            }

            try
            {
                if (communicationObject.State != CommunicationState.Faulted)
                {
                    communicationObject.Close();
                    isClosed = true;
                }
            }
            catch (CommunicationException)
            {
                // Catch this expected exception so it is not propagated further.
                // Perhaps write this exception out to log file for gathering statistics...
            }
            catch (TimeoutException)
            {
                // Catch this expected exception so it is not propagated further.
                // Perhaps write this exception out to log file for gathering statistics...
            }
            finally
            {
                // If State was Faulted or any exception occurred while doing the Close(), then do an Abort()
                if (!isClosed)
                {
                    AbortServiceChannel(communicationObject);
                }
            }
        }

        private static void AbortServiceChannel(ICommunicationObject communicationObject)
        {
            try
            {
                communicationObject.Abort();
            }
            catch (Exception)
            {
                // An unexpected exception that we don't know how to handle.
                // If we are in this situation:
                // - we should NOT retry the Abort() because it has already failed and there is nothing to suggest it could be successful next time
                // - the abort may have partially succeeded
                // - the actual service call may have been successful
                //
                // The only thing we can do is hope that the channel's resources have been released.
                // Do not rethrow this exception because the actual service operation call might have succeeded
                // and an exception closing the channel should not stop the client doing whatever it does next.
                //
                // Perhaps write this exception out to log file for gathering statistics and support purposes...
            }
        }
    }
}
