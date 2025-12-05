package com.example.amfootball.data.dtos.fcm

import com.google.gson.annotations.SerializedName

/**
 * Data Transfer Object (DTO) utilizado para enviar ou receber o Token de Dispositivo
 * (FCM - Firebase Cloud Messaging) de/para o backend.
 * * Este DTO é crucial para o endpoint de atualização de tokens, garantindo que o
 * dispositivo correto recebe notificações push.
 *
 * @property token O Token único de dispositivo fornecido pelo Firebase.
 * O campo é anotado com [SerializedName] para garantir que a serialização/desserialização
 * funcione corretamente, independentemente de o backend esperar "Token" (PascalCase) ou "token" (camelCase).
 */
data class DeviceTokenDto(
    @SerializedName("Token", alternate = ["token"])
    val token: String
)