package com.example.amfootball.ui.screens.Chat
import androidx.compose.foundation.background
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.foundation.shape.CircleShape
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.ArrowBack
import androidx.compose.material.icons.filled.MoreVert
import androidx.compose.material.icons.filled.Send
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.clip
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp

// --- Modelo de Dados (Permanece o mesmo) ---
data class GroupMessage(
    val id: String,
    val text: String,
    val senderName: String,
    val senderColor: Color,
    val isSentByMe: Boolean,
    val timestamp: String
)

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun GroupChatScreen() {
    // Cores para os jogadores
    val colorAdm = Color(0xFFE91E63) // Edu (Adm) - Rosa
    val colorGoleiro = Color(0xFF2196F3) // Goleiro - Azul
    val colorRiso = Color(0xFFFF9800) // O Engraçado - Laranja
    val colorMe = Color(0xFF4CAF50)   // Eu - Verde

    // --- DADOS ATUALIZADOS: TEMA FUTEBOL ---
    val mockMessages = listOf(
        GroupMessage("1", "Fala galera! Lista de Domingo. Quem vai?", "Luiz (Adm)", colorAdm, false, "09:30"),
        GroupMessage("2", "1 - Luiz\n2 - ...", "Luiz (Adm)", colorAdm, false, "09:30"),
        GroupMessage("3", "Tô dentro! 🙋‍♂️", "João", colorGoleiro, false, "09:32"),
        GroupMessage("4", "Coloca meu nome aí. O goleiro confirmou?", "Marcão", colorRiso, false, "09:35"),
        GroupMessage("5", "Confirmou sim, ele vem.", "Luiz (Adm)", colorAdm, false, "09:36"),
        GroupMessage("6", "Deixem somente ver se consigo!", "Eu", colorMe, true, "09:40"),
        GroupMessage("7", "Tambem irei.Podem contar comigo!", "Luiz (Adm)", colorAdm, false, "09:41"),
        GroupMessage("8", "Vou levar meu primo, joga muito.", "Marcão", colorRiso, false, "09:45"),
        GroupMessage("9", "Se jogar igual você tamo ferrado kkkkk", "João", colorGoleiro, false, "09:46"),
        GroupMessage("10", "Domingo não consigo, podemos remarcar?", "Eu", colorMe, true, "09:50")
    )

    var messageText by remember { mutableStateOf("") }

    Scaffold(
        topBar = {
            // --- TÍTULO ATUALIZADO ---
            GroupChatTopBar(
                groupName = "The Simpsons VS Gorilazz",
                participants = "Luiz, João, Antonio, Você, +5..."
            ) { }
        },
        content = { paddingValues ->
            Column(
                modifier = Modifier
                    .fillMaxSize()
                    .padding(paddingValues)
            ) {
                LazyColumn(
                    modifier = Modifier
                        .weight(1f)
                        .padding(horizontal = 16.dp),
                    verticalArrangement = Arrangement.spacedBy(8.dp)
                ) {
                    item { Spacer(modifier = Modifier.height(8.dp)) }
                    items(mockMessages) { message ->
                        GroupMessageBubble(message = message)
                    }
                    item { Spacer(modifier = Modifier.height(8.dp)) }
                }

                GroupMessageInput(
                    message = messageText,
                    onMessageChange = { messageText = it },
                    onSendClick = { messageText = "" }
                )
            }
        }
    )
}

// --- Os componentes visuais (Bubble, TopBar, Input) permanecem iguais ---
// Cole aqui as funções auxiliares (GroupChatTopBar, GroupMessageBubble, GroupMessageInput)
// do código anterior se elas não estiverem no mesmo arquivo.

// Vou repetir os componentes aqui para garantir que o código funcione ao copiar e colar:

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun GroupChatTopBar(groupName: String, participants: String, onBackClick: () -> Unit) {
    TopAppBar(
        title = {
            Row(verticalAlignment = Alignment.CenterVertically) {
                Surface(
                    shape = CircleShape,
                    color = MaterialTheme.colorScheme.primaryContainer,
                    modifier = Modifier.size(40.dp)
                ) {
                    Box(contentAlignment = Alignment.Center) {
                        Text("⚽", fontSize = 20.sp) // Ícone de bola
                    }
                }
                Spacer(modifier = Modifier.width(12.dp))
                Column {
                    Text(text = groupName, fontWeight = FontWeight.Bold, fontSize = 18.sp)
                    Text(
                        text = participants,
                        fontSize = 12.sp,
                        color = MaterialTheme.colorScheme.onSurfaceVariant,
                        maxLines = 1
                    )
                }
            }
        },
        navigationIcon = {
            IconButton(onClick = onBackClick) {
                Icon(Icons.Default.ArrowBack, contentDescription = "Voltar")
            }
        },
        actions = {
            IconButton(onClick = { }) {
                Icon(Icons.Default.MoreVert, contentDescription = "Opções")
            }
        },
        colors = TopAppBarDefaults.topAppBarColors(containerColor = MaterialTheme.colorScheme.surface)
    )
}

@Composable
fun GroupMessageBubble(message: GroupMessage) {
    val isMe = message.isSentByMe
    Row(
        modifier = Modifier.fillMaxWidth(),
        horizontalArrangement = if (isMe) Arrangement.End else Arrangement.Start,
        verticalAlignment = Alignment.Bottom
    ) {
        if (!isMe) {
            Box(
                modifier = Modifier
                    .size(32.dp)
                    .clip(CircleShape)
                    .background(message.senderColor),
                contentAlignment = Alignment.Center
            ) {
                Text(
                    text = message.senderName.first().toString(),
                    color = Color.White,
                    fontSize = 14.sp,
                    fontWeight = FontWeight.Bold
                )
            }
            Spacer(modifier = Modifier.width(8.dp))
        }
        Column(horizontalAlignment = if (isMe) Alignment.End else Alignment.Start) {
            if (!isMe) {
                Text(
                    text = message.senderName,
                    fontSize = 12.sp,
                    color = MaterialTheme.colorScheme.onSurfaceVariant,
                    modifier = Modifier.padding(start = 4.dp, bottom = 2.dp)
                )
            }
            Surface(
                shape = if (isMe) RoundedCornerShape(20.dp, 4.dp, 20.dp, 20.dp) else RoundedCornerShape(4.dp, 20.dp, 20.dp, 20.dp),
                color = if (isMe) MaterialTheme.colorScheme.primaryContainer else MaterialTheme.colorScheme.surfaceVariant,
                tonalElevation = 1.dp
            ) {
                Column(modifier = Modifier.padding(horizontal = 12.dp, vertical = 8.dp)) {
                    Text(
                        text = message.text,
                        color = if (isMe) MaterialTheme.colorScheme.onPrimaryContainer else MaterialTheme.colorScheme.onSurfaceVariant
                    )
                    Text(
                        text = message.timestamp,
                        fontSize = 10.sp,
                        color = if (isMe) MaterialTheme.colorScheme.onPrimaryContainer.copy(alpha = 0.7f) else MaterialTheme.colorScheme.onSurfaceVariant.copy(alpha = 0.7f),
                        modifier = Modifier.align(Alignment.End).padding(top = 4.dp)
                    )
                }
            }
        }
    }
}

@Composable
fun GroupMessageInput(message: String, onMessageChange: (String) -> Unit, onSendClick: () -> Unit) {
    Surface(tonalElevation = 2.dp) {
        Row(
            modifier = Modifier.fillMaxWidth().padding(8.dp),
            verticalAlignment = Alignment.CenterVertically
        ) {
            OutlinedTextField(
                value = message,
                onValueChange = onMessageChange,
                placeholder = { Text("Enviar mensagem...") },
                modifier = Modifier.weight(1f),
                shape = RoundedCornerShape(24.dp),
                maxLines = 3
            )
            Spacer(modifier = Modifier.width(8.dp))
            IconButton(
                onClick = onSendClick,
                modifier = Modifier.size(48.dp).clip(CircleShape).background(MaterialTheme.colorScheme.primary)
            ) {
                Icon(Icons.Default.Send, contentDescription = null, tint = MaterialTheme.colorScheme.onPrimary)
            }
        }
    }
}

@Preview(showBackground = true)
@Composable
fun GroupChatFootballPreview() {
    GroupChatScreen()
}