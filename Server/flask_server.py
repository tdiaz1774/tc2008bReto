from agent import *
from model import RandomModel
from flask import Flask, request, jsonify
from random import uniform

carModel = None 
currentStep = 0

# Coordinate server using Flask

app = Flask("Coordinate server")


@app.route('/init', methods=['POST', 'GET'])
def initModel():
    global num_agents

    if request.method == 'POST':
        num_agents = int(request.form.get("numAgents"))
        currentStep = 0
        print(f"Recieved num agentes = {num_agents}")
    
    global carModel 
    carModel = RandomModel(num_agents)
    return jsonify({"message":"Parameters recieved, model initiated."})

@app.route('/getAgents', methods=['GET'])
def getAgents():
    global carModel 

    directionStrToAngle = {
        "Right": {"x": 1, "y": 0, "z": 0},
        "Left": {"x": -1, "y": 0, "z": 0},
        "Up": {"x": 0, "y": 0, "z": 1},
        "Down": {"x": 0, "y": 0, "z": -1}
    }

    if request.method == 'GET':
        carPositions = []
        directions = []
        
        for agent in carModel.schedule.agents:
            if isinstance(agent, list):
                agent = agent[0]
            if isinstance(agent, Car):
                carPositions.append({"x": agent.pos[0], "y":0, "z":agent.pos[1]})
                directions.append(directionStrToAngle[agent.direction])

                

        return jsonify({'positions': carPositions, 'directions': directions})
#agentes del borde


@app.route('/getObstacles', methods=['GET'])
def getObstacles():
    global carModel

    if request.method == 'GET':
        roombaPositions = [{"x": x, "y":0, "z":z} for (a, x, z) in carModel.grid.coord_iter() if isinstance(a[0], Obstacle)]

        return jsonify({'positions':roombaPositions})

@app.route('/getTrafficLights', methods=['GET'])
def getTrafficLights():
    global carModel

    if request.method == 'GET':
        lightStatus = []
        for agent in carModel.schedule.agents:
            if isinstance(agent, list):
                agent = agent[0]
            if isinstance(agent, Traffic_Light):
                lightStatus.append(agent.state)
        return jsonify({'status':lightStatus})

@app.route('/getDestinations', methods=['GET'])
def getDestination():
    global carModel

    if request.method == 'GET':
        destinationPositions = [{"x": x, "y":0, "z":z} for (a, x, z) in carModel.grid.coord_iter() if isinstance(a[0], Destination)]

        return jsonify({'positions':destinationPositions})



@app.route('/update', methods=['GET'])
def updateModel():
    global currentStep, carModel
    if request.method == 'GET':
        carModel.step()
        currentStep += 1
        return jsonify({'message':f'Model updated to step {currentStep}.', 'currentStep':currentStep})


if __name__=='__main__':
    app.run(host="localhost", port=8585, debug=True)
