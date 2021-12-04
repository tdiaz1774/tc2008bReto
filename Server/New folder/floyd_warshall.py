import time

# Print iterations progress
def printProgressBar (iteration, total, prefix = '', suffix = '', decimals = 1, length = 100, fill = '█', printEnd = "\r"):
    """
    Call in a loop to create terminal progress bar
    @params:
        iteration   - Required  : current iteration (Int)
        total       - Required  : total iterations (Int)
        prefix      - Optional  : prefix string (Str)
        suffix      - Optional  : suffix string (Str)
        decimals    - Optional  : positive number of decimals in percent complete (Int)
        length      - Optional  : character length of bar (Int)
        fill        - Optional  : bar fill character (Str)
        printEnd    - Optional  : end character (e.g. "\r", "\r\n") (Str)
    """
    percent = ("{0:." + str(decimals) + "f}").format(100 * (iteration / float(total)))
    filledLength = int(length * iteration // total)
    bar = fill * filledLength + '-' * (length - filledLength)
    print(f'\r{prefix} |{bar}| {percent}% {suffix}', end = printEnd)
    # Print New Line on Complete
    if iteration == total: 
        print()

# Algorithm 
def floyd(G, nV, INF):
    dist = list(map(lambda p: list(map(lambda q: q, p)), G))
    next = []

    for i in range(nV):
        newList = []

        for k in range(nV):
            if dist[i][k] != INF:
                newList.append(k)
            else:
                newList.append(-1)
        next.append(newList)

    # Adding vertices individually
    for r in range(nV):
        for p in range(nV):
            for q in range(nV):
                if dist[p][q] > dist[p][r] + dist[r][q]:
                    dist[p][q] = dist[p][r] + dist[r][q]
                    next[p][q] = next[p][r]
        
        printProgressBar(r,nV,prefix="Mapping:",suffix="Complete",length=70)

    return next, dist

# Searching an specific route
def floyd_route(p1, p2, next, maxSteps):
    result = []
    i = p1
    nextNode = 0
    steps = 0

    while True:
        nextNode = next[i][p2]
        result.append((i,nextNode))
        
        i = nextNode

        if i == p2:
            break

        if steps > maxSteps:
            print("ERROR: COULD NOT GET THE ROUTE")
            return [[0,0]]

        steps += 1
    
    return result

# Printing the output
def sol(dist):
    for p in range(nV):
        for q in range(nV):
            if(dist[p][q] == INF):
                print("INF", end=" ")
            else:
                print(dist[p][q], end="  ")
        print(" ")

