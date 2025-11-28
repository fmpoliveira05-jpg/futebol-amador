package com.example.amfootball.data.dtos.team

import android.net.Uri
import com.example.amfootball.data.dtos.rank.RankNameDto
import com.google.gson.annotations.SerializedName

/**
 * Data Transfer Object (DTO) que representa as informações detalhadas e resumidas de uma Equipa.
 *
 * Este objeto é utilizado para mapear a resposta JSON da API para o modelo da aplicação,
 * lidando com diferenças de nomes de campos através de [@SerializedName] e fornecendo
 * propriedades computadas para facilitar a exibição na UI.
 *
 * @property id O identificador único da equipa (UUID).
 * @property name O nome da equipa.
 * @property logoTeam A URI do logótipo da equipa. Padrão é [Uri.EMPTY] se não fornecido.
 * @property description Uma breve descrição ou lema da equipa.
 * @property fullAddress O endereço completo recebido da API (campo JSON "address"). Ex: "Rua das Flores, Porto".
 * @property rank O objeto contendo o ID e o nome do rank atual (ex: Ouro, Prata).
 * @property points A pontuação atual da equipa (campo JSON "currentPoints").
 * @property averageAge A idade média dos membros da equipa (em formato decimal).
 * @property numberMembers O número total de jogadores na equipa (campo JSON "playerCount").
 */
data class ItemTeamInfoDto(
    val id: String,
    val name: String,
    val logoTeam: Uri = Uri.EMPTY,
    val description: String,
    @SerializedName("address")
    val fullAddress: String,
    val rank: RankNameDto,
    @SerializedName("currentPoints")
    val points: Int,
    val averageAge: Double,
    @SerializedName("playerCount")
    val numberMembers: Int
) {
    /**
     * Propriedade computada que extrai o nome da cidade a partir do [fullAddress].
     *
     * Lógica de extração:
     * Assume que a cidade é a última parte do endereço após a última vírgula.
     * Exemplo: "Av. da Liberdade, Lisboa" -> Retorna "Lisboa".
     *
     * @return O nome da cidade ou "Sem Localização" se o endereço estiver vazio.
     */
    val city: String
        get() {
            if (fullAddress.isNullOrBlank()) return "Sem Localização"

            return fullAddress.substringAfterLast(",").trim()
        }
}