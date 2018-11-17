import numpy as np
import matplotlib.pyplot as plt


def func(x):
    return 20*np.sin(x)*np.cos(x/10+20)  

x = np.arange(1,10,0.01)

tarX = 5
tarY = 1

y = func(x)
y2 = func(tarX)

plt.plot(x,y)
plt.plot(tarX,tarY - y2,'o')
print(tarY - y2)

