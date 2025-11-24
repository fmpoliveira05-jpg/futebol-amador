package com.example.amfootball.utils.extensions

import java.time.Instant
import java.time.LocalDateTime
import java.time.ZoneId

/**
 * Converte um timestamp (valor em milissegundos) do tipo [Long] para um objeto [LocalDateTime].
 *
 * É uma função de extensão, o que permite chamá-la diretamente em qualquer variável Long
 * (ex: `dataMillis.toLocalDateTime()`).
 *
 * O método assume o fuso horário (Time Zone) padrão do sistema para realizar a conversão,
 * transformando os milissegundos UTC (época) para uma data e hora locais.
 *
 * @receiver O timestamp em milissegundos desde a época (epoch, 1 de Janeiro de 1970 UTC).
 * @return Um objeto [LocalDateTime] que representa a data e hora no fuso horário do sistema.
 */
fun Long.toLocalDateTime(): LocalDateTime {
    return Instant.ofEpochMilli(this)
        .atZone(ZoneId.systemDefault())
        .toLocalDateTime()
}