FROM public.ecr.aws/lambda/dotnet:8-preview.2023.12.15.17-arm64

COPY publish/* ${LAMBDA_TASK_ROOT}
  
# Set the CMD to your handler (could also be done as a parameter override outside of the Dockerfile)
CMD [ "Maestro.Events.Runner" ]