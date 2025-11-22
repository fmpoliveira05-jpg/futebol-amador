package com.example.amfootball.ui.components.lists

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.PaddingValues
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.Inbox
import androidx.compose.material3.ElevatedCard
import androidx.compose.material3.Icon
import androidx.compose.material3.ListItem
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Surface
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.res.stringResource
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.text.style.TextOverflow
import androidx.compose.ui.unit.dp
import com.example.amfootball.R
import com.example.amfootball.ui.components.buttons.AcceptButton
import com.example.amfootball.ui.components.buttons.RejectButton
import com.example.amfootball.ui.components.buttons.ShowMoreInfoButton

/**
 * Container principal que exibe uma lista dinâmica de itens ([LazyColumn]) e um cabeçalho de filtro.
 *
 * Lida com a exibição condicional: se a lista estiver vazia, mostra um ícone e uma mensagem.
 * Caso contrário, itera sobre os itens e exibe-os.
 *
 * @param T O tipo de dados dos itens na lista.
 * @param list A lista de dados a ser exibida.
 * @param filterSection Slot Composable para a secção de filtros (geralmente [FilterSection]).
 * @param listItems Slot Composable para renderizar a UI de um item individual da lista.
 * @param itemSpacing O espaçamento vertical entre os itens da lista em dp (padrão: 12).
 * @param messageEmptyList A mensagem a ser exibida quando a lista estiver vazia.
 */
@Composable
fun<T> ListSurface(
    list: List<T>,
    filterSection: @Composable () -> Unit,
    listItems: @Composable (T) -> Unit,
    itemSpacing: Int = 12,
    messageEmptyList: String = "asd"
) {
    Surface {
        LazyColumn(
            contentPadding = PaddingValues(horizontal = 16.dp, vertical = 8.dp)
        ) {
            item {
                filterSection()
                Spacer(Modifier.height(16.dp))
            }

            if (list.isEmpty()) {
                item {
                    Column(
                        modifier = Modifier
                            .fillMaxWidth()
                            .padding(vertical = 64.dp),
                        horizontalAlignment = Alignment.CenterHorizontally,
                        verticalArrangement = Arrangement.spacedBy(16.dp)
                    ) {
                        Icon(
                            imageVector = Icons.Default.Inbox,
                            contentDescription = null,
                            modifier = Modifier.size(72.dp),
                            tint = MaterialTheme.colorScheme.onSurfaceVariant
                        )

                        Text(
                            text = messageEmptyList,
                            style = MaterialTheme.typography.headlineSmall,
                            color = MaterialTheme.colorScheme.onSurfaceVariant,
                            textAlign = TextAlign.Center
                        )
                    }
                }
            } else {
                items(list) { item ->
                    listItems(item)
                    Spacer(Modifier.height(itemSpacing.dp))
                }
            }
        }
    }
}

/**
 * Container genérico para um item individual da lista, estilizado como um [ElevatedCard].
 *
 * Utiliza o [ListItem] do Material 3 e oferece slots customizados para título,
 * conteúdo à esquerda (leading), conteúdo de suporte e rodapé opcional.
 *
 * @param T O tipo de dados do item.
 * @param item O objeto de dados a ser exibido.
 * @param title Função que extrai a string principal do título a partir do item.
 * @param overline Slot Composable para exibir conteúdo sobre o título.
 * @param leading Slot Composable que exibe o conteúdo inicial (ex: imagem de perfil).
 * @param supporting Slot Composable que exibe informações adicionais abaixo do título.
 * @param trailing Slot Composable que exibe conteúdo no final da linha (ex: botões de ação).
 * @param underneathItems Slot Composable opcional para exibir conteúdo abaixo do [ListItem] (ex: uma linha de botões).
 */
@Composable
fun<T> GenericListItem(
    item: T,
    title: (T) -> String,
    overline: @Composable () -> Unit = {},
    leading: @Composable (T) -> Unit = {},
    supporting: @Composable (T) -> Unit = {},
    trailing: @Composable () -> Unit = {},
    underneathItems: (@Composable () -> Unit)? = null
) {
    ElevatedCard(modifier = Modifier.fillMaxWidth()) {
        Column {
            ListItem(
                headlineContent = {
                    Text(
                        text = title(item),
                        style = MaterialTheme.typography.titleLarge,
                        maxLines = 2,
                        overflow = TextOverflow.Ellipsis
                    )
                },
                overlineContent = {
                    overline()
                },
                supportingContent = {
                    supporting(item)
                },
                leadingContent = {
                    leading(item)
                },
                trailingContent = {
                    trailing()
                }
            )
        }

        if (underneathItems != null) {
            Row(
                verticalAlignment = Alignment.CenterVertically,
                horizontalArrangement = Arrangement.Center,
                modifier = Modifier
                    .fillMaxWidth()
                    .padding(horizontal = 8.dp, vertical = 4.dp)
            ) {
                underneathItems()
            }
        }
    }
}

/**
 * Um agrupador de botões para ações comuns de listas, como pedidos de adesão ou detalhes.
 *
 * Exibe os botões [AcceptButton], [RejectButton] e [ShowMoreInfoButton] lado a lado.
 *
 * @param accept Callback executado ao pressionar o botão de aceitar.
 * @param reject Callback executado ao pressionar o botão de rejeitar.
 * @param showMore Callback executado ao pressionar o botão de ver mais detalhes.
 */
@Composable
fun ItemAcceptRejectAndShowMore(
    accept: () -> Unit,
    reject: () -> Unit,
    showMore: () -> Unit,
) {
    Row(
        verticalAlignment = Alignment.CenterVertically,
        horizontalArrangement = Arrangement.spacedBy(4.dp),
        modifier = Modifier.padding(start = 8.dp)
    ) {
        AcceptButton(accept = accept)

        RejectButton(reject = reject)

        ShowMoreInfoButton(
            showMoreDetails = showMore,
            contentDescription = stringResource(id = R.string.list_teams_view_team)
        )
    }
}