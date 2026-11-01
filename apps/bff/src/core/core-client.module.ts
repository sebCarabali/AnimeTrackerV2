import { Module } from '@nestjs/common';
import { ConfigService } from '@nestjs/config';
import { CORE_CLIENT, createCoreClient } from './core-client';

@Module({
  providers: [
    {
      provide: CORE_CLIENT,
      inject: [ConfigService],
      useFactory: (configService: ConfigService) =>
        createCoreClient(configService.getOrThrow<string>('CORE_BASE_URL')),
    },
  ],
  exports: [CORE_CLIENT],
})
export class CoreClientModule {}
