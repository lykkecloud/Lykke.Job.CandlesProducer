FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-alpine
WORKDIR /app
COPY . .
RUN apk add --no-cache icu-libs \
&& cp cert/*.crt /usr/local/share/ca-certificates/ 2>/dev/null \
&& update-ca-certificates \
|| echo "No specific certificate given."
RUN apk --update --no-cache add mono libgdiplus-dev bash\
 && rm -rf /tmp/* /var/cache/apk/*
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
ENTRYPOINT ["dotnet", "Lykke.Job.CandlesProducer.dll"]