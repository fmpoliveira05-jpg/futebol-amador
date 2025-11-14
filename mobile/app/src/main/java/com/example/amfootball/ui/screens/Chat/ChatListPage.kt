package com.example.amfootball.ui.screens.Chat

import androidx.compose.foundation.lazy.items
import androidx.compose.foundation.background
import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.shape.CircleShape
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.Add
import androidx.compose.material.icons.filled.Menu
import androidx.compose.material.icons.filled.Search
import androidx.compose.material3.*
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.clip
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.style.TextOverflow
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
// Certifique-se de ter uma imagem de placeholder no seu drawable (R.drawable.avatar_placeholder)
// ou remova a parte da imagem se não tiver.

// --- Modelo de Dados ---
data class Chat(
    val id: Int,
    val name: String,
    val lastMessage: String,
    val time: String,
    val unreadCount: Int = 0,
    val isOnline: Boolean = false
)

// --- Dados Fictícios (Mock Data) ---
val sampleChats = listOf(
    Chat(1, "FooFigthers VS Gorilazz", "João: É possivel remarcar para domingo as 10?", "10:30", 2, true),
    Chat(2, "The Simpsons VS Friends", "Antonio: Sexta-feira não poderei.", "09:15", 5),
    Chat(3, "João Dev", "O PR foi aprovado, pode fazer o merge.", "Ontem", 0, true),
    Chat(4, "Suporte Técnico", "Seu ticket #4829 foi resolvido.", "Ontem", 0),
    Chat(5, "Maria Oliveira", "Enviei as fotos do projeto.", "Segunda", 1),
    Chat(6, "Bruno Academia", "Bora treinar hoje?", "Domingo", 0),
    Chat(7, "Clube do Livro", "A próxima leitura será 'Duna'.", "Sábado", 12),
    Chat(8, "Lucas Design", "Vou te mandar o Figma atualizado.", "Sexta", 0, true)
)

// --- Tela Principal ---
@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun ChatListScreen() {
    Scaffold(
        topBar = {
            TopAppBar(
                title = {
                    Text(
                        text = "Conversas",
                        fontWeight = FontWeight.Bold,
                        fontSize = 22.sp
                    )
                },
                actions = {
                    IconButton(onClick = { /* Ação de busca */ }) {
                        Icon(Icons.Default.Search, contentDescription = "Buscar")
                    }
                    IconButton(onClick = { /* Menu */ }) {
                        Icon(Icons.Default.Menu, contentDescription = "Menu")
                    }
                },
                colors = TopAppBarDefaults.topAppBarColors(
                    containerColor = MaterialTheme.colorScheme.primary,
                    titleContentColor = Color.White,
                    actionIconContentColor = Color.White
                )
            )
        },
        floatingActionButton = {
            FloatingActionButton(
                onClick = { /* Nova conversa */ },
                containerColor = MaterialTheme.colorScheme.secondaryContainer,
                contentColor = MaterialTheme.colorScheme.onSecondaryContainer
            ) {
                Icon(Icons.Default.Add, contentDescription = "Nova Conversa")
            }
        }
    ) { paddingValues ->
        // Lista de Chats
        LazyColumn(
            modifier = Modifier
                .fillMaxSize()
                .padding(paddingValues)
                .background(MaterialTheme.colorScheme.background)
        ) {
            items(sampleChats) { chat ->
                ChatItem(chat)
                HorizontalDivider(
                    modifier = Modifier.padding(start = 80.dp),
                    thickness = 0.5.dp,
                    color = Color.LightGray.copy(alpha = 0.4f)
                )
            }
        }
    }
}

// --- Componente de Item da Lista ---
@Composable
fun ChatItem(chat: Chat) {
    Row(
        modifier = Modifier
            .fillMaxWidth()
            .clickable { /* Abrir chat */ }
            .padding(horizontal = 16.dp, vertical = 12.dp),
        verticalAlignment = Alignment.CenterVertically
    ) {
        // Avatar com indicador de Online
        Box {
            // Placeholder simples para o Avatar (Use AsyncImage do Coil em app real)
            Surface(
                modifier = Modifier.size(56.dp),
                shape = CircleShape,
                color = MaterialTheme.colorScheme.primaryContainer
            ) {
                Box(contentAlignment = Alignment.Center) {
                    Text(
                        text = chat.name.first().toString(),
                        fontSize = 24.sp,
                        fontWeight = FontWeight.SemiBold,
                        color = MaterialTheme.colorScheme.onPrimaryContainer
                    )
                }
            }

            // Bolinha Verde (Online)
            if (chat.isOnline) {
                Box(
                    modifier = Modifier
                        .size(14.dp)
                        .background(Color.Green, CircleShape)
                        .align(Alignment.BottomEnd)
                        .clip(CircleShape)
                        .background(Color.White) // Borda branca falsa
                        .padding(2.dp)
                        .background(Color(0xFF4CAF50), CircleShape)
                )
            }
        }

        Spacer(modifier = Modifier.width(16.dp))

        // Coluna Central (Nome e Mensagem)
        Column(modifier = Modifier.weight(1f)) {
            Text(
                text = chat.name,
                style = MaterialTheme.typography.bodyLarge,
                fontWeight = FontWeight.SemiBold,
                color = MaterialTheme.colorScheme.onBackground
            )
            Spacer(modifier = Modifier.height(4.dp))
            Text(
                text = chat.lastMessage,
                style = MaterialTheme.typography.bodyMedium,
                color = Color.Gray,
                maxLines = 1,
                overflow = TextOverflow.Ellipsis
            )
        }

        Spacer(modifier = Modifier.width(8.dp))

        // Coluna Lateral (Hora e Contador)
        Column(horizontalAlignment = Alignment.End) {
            Text(
                text = chat.time,
                style = MaterialTheme.typography.labelSmall,
                color = if (chat.unreadCount > 0) MaterialTheme.colorScheme.primary else Color.Gray
            )

            Spacer(modifier = Modifier.height(6.dp))

            if (chat.unreadCount > 0) {
                Surface(
                    color = MaterialTheme.colorScheme.primary,
                    shape = CircleShape,
                    modifier = Modifier.height(20.dp).defaultMinSize(minWidth = 20.dp)
                ) {
                    Box(contentAlignment = Alignment.Center, modifier = Modifier.padding(horizontal = 6.dp)) {
                        Text(
                            text = chat.unreadCount.toString(),
                            color = Color.White,
                            style = MaterialTheme.typography.labelSmall,
                            fontWeight = FontWeight.Bold
                        )
                    }
                }
            }
        }
    }
}

// --- Preview para o Android Studio ---
@Preview(showBackground = true, showSystemUi = true)
@Composable
fun ChatListScreenPreview() {
    MaterialTheme {
        ChatListScreen()
    }
}

