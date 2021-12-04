from mesa import Model
from mesa.time import RandomActivation
from mesa.space import MultiGrid
from agent import *
from floyd_warshall import floyd, floyd_route
from floyd_map import floyd_map, floyd_lines
import json
import math

class RandomModel(Model):
    """ 
    Creates a new model with random agents.
    Args:
        N: Number of agents in the simulation
        height, width: The size of the grid to model
    """
    def __init__(self, N):
        #coordenadas destinos del mapa
        self.destinys = []
        #coordenadas del road
        self.roads = []

        self.numAgents = N
        self.maxRouteLength = 100
        lines = []

        dataDictionary = json.load(open("mapDictionary.txt"))

        with open('base.txt') as baseFile:
            lines = baseFile.readlines()
            self.width = len(lines[0])-1
            self.height = len(lines)            
            
            self.grid = MultiGrid(self.width, self.height,torus = False) 
            self.schedule = RandomActivation(self)

            for r, row in enumerate(lines):
                for c, col in enumerate(row):
                    
                    #agregas direcciones al objeto road
                    if col in ["v", "^", ">", "<"]:
                        agent = Road(f"r{r*self.width+c}", self, dataDictionary[col])
                        self.roads.append((c, self.height - r - 1))
                        self.grid.place_agent(agent, (c, self.height - r - 1))
                    
                    #semaforo vertical u horizontal
                    elif col in ["S", "s"]:
                        agent = Traffic_Light(f"tl{r*self.width+c}", self, False if col == "S" else True, int(dataDictionary[col]))
                        self.grid.place_agent(agent, (c, self.height - r - 1))
                        self.schedule.add(agent)
                    
                    #obstaculo
                    elif col == "#":
                        agent = Obstacle(f"ob{r*self.width+c}", self)
                        self.grid.place_agent(agent, (c, self.height - r - 1))
                    
                    #destino
                    elif col == "D":
                        agent = Destination(f"d{r*self.width+c}", self)
                        #agrega el destino a una lista
                        self.destinys.append(agent)
                        self.grid.place_agent(agent, (c, self.height - r - 1))


        lightDctnry = {
            7: {
                "Coords": [(-1,0), (1,0)],
                "Directions": ["Right","Left"]
            },
            15: {
                "Coords": [(0,-1), (0,1)],
                "Directions": ["Up","Down"]
            }
        }


        # Si las lineas del archivo no son iguales a las almacenadas en el algoritmo hacemos un nuevo mapa
        if lines != floyd_lines:
            # Inicializar el algoritmo de Floyd-Warshall
            INF = 999 # Definimos un número que emule al infinito en el algoritmo de Floyd-Warshall
            G = [] # Inicializamos la matriz que emula un grafo en el algoritmo de Floyd-Warshall

            for _ in range(self.height**2):
                newList = []
                for _ in range(self.width**2):
                    newList.append(INF)

                G.append(newList)
            
            moveDctnry = {
                "Right": 1,
                "Left": -1,
                "Up": 1,
                "Down": -1
            }
        
            move = 0
            newCoords = (0,0)

            for (a,x,y) in self.grid.coord_iter():
                if isinstance(a, list):
                    a = a[0]

                if isinstance(a, Traffic_Light):
                    while a.direction == "":
                        
                        for i in range(2):
                            newCoords = (x+lightDctnry[a.timeToChange]["Coords"][i][0], y+lightDctnry[a.timeToChange]["Coords"][i][1])
                            # Copiar la dirección del vecino
                            if isinstance(self.grid[newCoords[0]][newCoords[1]][0], Road) and self.grid[newCoords[0]][newCoords[1]][0].direction in lightDctnry[a.timeToChange]["Directions"]:
                                a.direction = self.grid[newCoords[0]][newCoords[1]][0].direction 

                if isinstance(a, Road) or isinstance(a, Traffic_Light):
                    G[x+self.width*y][x+self.width*y] = 0
                    move = moveDctnry[a.direction]

                    if (a.direction == "Right" or a.direction == "Left") and x+move > 0 and x+move < self.width and isinstance(self.grid[x+move][y], list):
                        G[x+self.width*y][x+self.width*y+move] = 1

                        if y-1 > 0 and isinstance(self.grid[x+move][y-1], list):
                            G[x+self.width*y][x+self.width*(y-1)+move] = INF/2
                        
                        if y+1 < self.height and isinstance(self.grid[x+move][y+1], list):
                            G[x+self.width*y][x+self.width*(y+1)+move] = INF/2

                    elif (a.direction == "Up" or a.direction == "Down") and y+move > 0 and y+move < self.height and isinstance(self.grid[x][y+move], list):
                        G[x+self.width*y][x+self.width*(y+move)] = 1

                        if x-1 > 0 and isinstance(self.grid[x-1][y+move], list):
                            G[x+self.width*y][x+self.width*(y+move)-1] =  INF/2
                        
                        if x+1 < self.width and isinstance(self.grid[x+1][y+move], list):
                            G[x+self.width*y][x+self.width*(y+move)+1] = INF/2
            
            self.next, dist = floyd(G, self.width*self.height, INF)
            file = open("floyd_map.py", "w")
            file.write("floyd_map = " + repr(self.next) + "\n\n" + "floyd_lines = " + repr(lines) + "\n")

            file.close()

        else:
            self.next = floyd_map

            newCoords = (0,0)

            for (a,x,y) in self.grid.coord_iter():
                if isinstance(a, list):
                    a = a[0]

                if isinstance(a, Traffic_Light):
                    while a.direction == "":
                        
                        for i in range(2):
                            newCoords = (x+lightDctnry[a.timeToChange]["Coords"][i][0], y+lightDctnry[a.timeToChange]["Coords"][i][1])
                            # Copiar la dirección del vecino
                            if isinstance(self.grid[newCoords[0]][newCoords[1]][0], Road) and self.grid[newCoords[0]][newCoords[1]][0].direction in lightDctnry[a.timeToChange]["Directions"]:
                                a.direction = self.grid[newCoords[0]][newCoords[1]][0].direction 

        self.num_agents = N

        #inicializar agentes
        for i in range(0,self.numAgents):
        #tomar posicion random de las celdas de roads
            while True:
                pos = self.roads[self.random.randrange(0,len(self.roads))]
                a = Car(i+1000, self, pos)
                if len(a.route) > 1:
                    self.grid.place_agent(a,pos)
                    self.schedule.add(a)
                    break
        self.running = True 

        

    def step(self):
        '''Advance the model by one step.'''
        self.schedule.step()
        if self.schedule.steps % 10 == 0:
            for agents, x, y in self.grid.coord_iter():
                for agent in agents:
                    if isinstance(agent, Traffic_Light):
                        agent.state = not agent.state

    def searchRoute(self, p1, p2):
        print(p1,p2)
        p1 = p1[1]*self.width + p1[0]
        p2 = p2[1]*self.width + p2[0]
        print("New p's: ", p1, p2)
        route = floyd_route(p1,p2,self.next,self.maxRouteLength)

        for i in range(len(route)):
            route[i] = (route[i][1]-self.width*math.floor(route[i][1]/self.width), math.floor(route[i][1]/self.width))
        
        return route
    
    def getClosestRoad(self, pos):
        possible_steps = self.grid.get_neighborhood(
            pos,
            #asi solo obtendra arriba,abajo,izq, der
            moore=False, # Boolean for whether to use Moore neighborhood (including diagonals) or Von Neumann (only up/down/left/right).
            include_center=True) 

        for possible in possible_steps:
            for agent in self.grid.get_cell_list_contents(possible):
                if (isinstance(agent, Road)):
                    return possible
